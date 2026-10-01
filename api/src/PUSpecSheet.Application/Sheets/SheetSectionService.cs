using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets;

public sealed class SheetSectionService(
    PuSpecSheetDbContext db,
    SectionDrafts drafts,
    LiveSectionQuery liveSections,
    SheetInstantiator instantiator,
    SheetReader reader,
    ICurrentUser currentUser) : ISheetSectionService
{
    public async Task<SheetDto> AddAsync(int tableId, AddSheetSectionRequest request, CancellationToken cancellationToken)
    {
        var table = await db.SheetTables
            .SingleOrDefaultAsync(candidate => candidate.Id == tableId, cancellationToken)
            ?? throw new NotFoundException($"Table {tableId} was not found.");

        var tree = await TemplateTree.LoadAsync(db, table.TableTemplateVersionId, cancellationToken);
        var template = tree.Find(request.TemplateSectionId)
            ?? throw new InvalidRequestException("That section isn't part of this table's template.");
        if (template.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("The header comes with the table and can't be added again.");
        }

        if (request.ParentSheetSectionId is { } parentId)
        {
            var parent = await db.SheetSections
                .AsNoTracking()
                .SingleOrDefaultAsync(candidate => candidate.Id == parentId && candidate.SheetTableId == tableId, cancellationToken)
                ?? throw new InvalidRequestException("That parent section isn't on this table.");
            if (parent.TemplateSectionId != template.ParentSectionId)
            {
                throw new InvalidRequestException($"'{template.Name}' can't be added inside that section.");
            }
        }
        else if (template.ParentSectionId is not null)
        {
            throw new InvalidRequestException($"'{template.Name}' goes inside another section.");
        }

        var siblings = (await liveSections.VisibleAsync(tableId, cancellationToken))
            .Where(section => section.ParentSheetSectionId == request.ParentSheetSectionId)
            .ToList();
        var copies = siblings.Count(section => section.TemplateSectionId == template.Id);
        if (template.MaxInstances is { } max && copies >= max)
        {
            throw new ConflictException($"'{template.Name}' can have at most {max}.");
        }

        var order = OrderGaps.Next(siblings.Select(section => section.DisplayOrder));
        var section = instantiator.NewSection(table, template, tree, order);
        section.ParentSheetSectionId = request.ParentSheetSectionId;

        db.SheetSections.Add(section);
        await db.SaveSheetChangesAsync(cancellationToken);
        return await reader.ReadLiveAsync(table.SheetId, cancellationToken);
    }

    public async Task<SheetDto> MoveAsync(int sectionId, MoveRequest request, CancellationToken cancellationToken)
    {
        var section = await LoadSectionAsync(sectionId, cancellationToken);
        if (section.TemplateSection.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("The header always stays first.");
        }

        var state = await drafts.LoadAsync(sectionId, cancellationToken);
        var siblingOrders = (await liveSections.VisibleAsync(section.SheetTableId, cancellationToken))
            .Where(candidate => candidate.ParentSheetSectionId == section.ParentSheetSectionId && candidate.Id != sectionId)
            .Select(candidate => candidate.DisplayOrder)
            .Order()
            .ToList();
        var order = OrderGaps.PlaceAt(siblingOrders, request.DisplayOrder);

        var (draft, _) = await drafts.EnsureMineAsync(sectionId, state, cancellationToken);
        draft.DisplayOrder = order;

        await db.SaveSheetChangesAsync(cancellationToken);
        return await reader.ReadLiveAsync(section.SheetTable.SheetId, cancellationToken);
    }

    public async Task<SheetDto> RemoveAsync(int sectionId, CancellationToken cancellationToken)
    {
        var section = await LoadSectionAsync(sectionId, cancellationToken);
        var template = section.TemplateSection;
        if (template.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("The header can't be removed.");
        }

        var siblings = (await liveSections.VisibleAsync(section.SheetTableId, cancellationToken))
            .Where(candidate => candidate.ParentSheetSectionId == section.ParentSheetSectionId
                && candidate.TemplateSectionId == section.TemplateSectionId)
            .ToList();
        if (siblings.Count <= template.MinInstances)
        {
            throw new ConflictException($"'{template.Name}' needs at least {template.MinInstances}.");
        }

        var state = await drafts.LoadAsync(sectionId, cancellationToken);
        await drafts.EnsureNotLockedByOthersAsync(state, cancellationToken);
        await EnsureNobodyElseIsEditingInsideAsync(section, cancellationToken);

        var sheetId = section.SheetTable.SheetId;
        if (state.IsNew)
        {
            await DeleteSubtreeAsync(section, cancellationToken);
        }
        else
        {
            var (draft, _) = await drafts.EnsureMineAsync(sectionId, state, cancellationToken);
            draft.IsDeleted = true;
            await db.SaveSheetChangesAsync(cancellationToken);
        }

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private async Task<SheetSection> LoadSectionAsync(int sectionId, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .Include(section => section.TemplateSection)
            .Include(section => section.SheetTable)
            .SingleOrDefaultAsync(section => section.Id == sectionId, cancellationToken)
            ?? throw new NotFoundException($"Section {sectionId} was not found.");
    }

    /// <summary>Removing a section removes what's inside it, so nobody else may be holding a draft in there.</summary>
    private async Task EnsureNobodyElseIsEditingInsideAsync(SheetSection section, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var ids = await liveSections.SubtreeIdsAsync(section.SheetTableId, section.Id, cancellationToken);

        var others = await db.SheetSectionRevisions
            .AnyAsync(revision => ids.Contains(revision.SheetSectionId) && revision.Status == RevisionStatus.Draft && revision.AuthorUserId != me, cancellationToken)
            || await db.SheetRowRevisions
                .AnyAsync(revision => ids.Contains(revision.SheetRow.SheetSectionId) && revision.Status == RevisionStatus.Draft && revision.AuthorUserId != me, cancellationToken);
        if (others)
        {
            throw new ConflictException("Someone else is editing inside this section, so it can't be removed yet.");
        }
    }

    /// <summary>
    /// Deletes a section only its author ever saw, along with its sub-sections. A section can't cascade
    /// to its own children in SQL Server, so they're deleted deepest first.
    /// </summary>
    private async Task DeleteSubtreeAsync(SheetSection section, CancellationToken cancellationToken)
    {
        var ids = await liveSections.SubtreeIdsAsync(section.SheetTableId, section.Id, cancellationToken);
        var parents = await db.SheetSections
            .AsNoTracking()
            .Where(candidate => ids.Contains(candidate.Id))
            .Select(candidate => new { candidate.Id, candidate.ParentSheetSectionId })
            .ToListAsync(cancellationToken);

        var remaining = parents.ToDictionary(entry => entry.Id, entry => entry.ParentSheetSectionId);
        while (remaining.Count > 0)
        {
            var leaves = remaining.Keys
                .Where(id => !remaining.ContainsValue(id))
                .ToList();
            await db.SheetSections.Where(candidate => leaves.Contains(candidate.Id)).ExecuteDeleteAsync(cancellationToken);
            foreach (var leaf in leaves)
            {
                remaining.Remove(leaf);
            }
        }
    }
}
