using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Layout;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Editing;

public sealed class SheetSectionService(
    ISheetTableRepository tables,
    ISheetSectionRepository sections,
    TemplateRepositories templates,
    IUnitOfWork unitOfWork,
    SectionDrafts drafts,
    LiveSectionQuery liveSections,
    SheetInstantiator instantiator,
    ISheetCellFiller filler,
    NewRowSettings newRowSettings,
    SheetReader reader,
    RemovalContentsGuard contents,
    IRemovalInProgressGuard removals,
    ICurrentUser currentUser) : ISheetSectionService
{
    public async Task<SheetDto> AddAsync(int tableId, AddSheetSectionRequest request, CancellationToken cancellationToken)
    {
        var table = await tables.FindForEditAsync(tableId, cancellationToken)
            ?? throw new NotFoundException($"Table {tableId} was not found.");

        var tree = await TemplateTree.LoadAsync(templates.Sections, templates.ColumnBlocks, table.TableTemplateVersionId, cancellationToken);
        var template = tree.Find(request.TemplateSectionId)
            ?? throw new InvalidRequestException("That section isn't part of this table's template.");
        if (template.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("The header comes with the table and can't be added again.");
        }

        await EnsureParentFitsAsync(tableId, template, request.ParentSheetSectionId, cancellationToken);
        await removals.EnsurePlaceNotBeingRemovedAsync(tableId, request.ParentSheetSectionId, currentUser.UserId, cancellationToken);
        var order = await NextOrderWithinLimitAsync(tableId, template, request.ParentSheetSectionId, cancellationToken);
        var section = instantiator.NewSection(table, template, tree, order);
        section.ParentSheetSectionId = request.ParentSheetSectionId;

        sections.Add(section);
        await unitOfWork.SaveSheetChangesAsync(cancellationToken);
        await filler.FillAsync(tableId, cancellationToken);
        await newRowSettings.InheritAsync(section, cancellationToken);
        return await reader.ReadLiveAsync(table.SheetId, cancellationToken);
    }

    public async Task<SheetDto> MoveAsync(int sectionId, MoveRequest request, CancellationToken cancellationToken)
    {
        var section = await LoadSectionAsync(sectionId, cancellationToken);
        if (section.TemplateSection.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("The header always stays first.");
        }

        await removals.EnsurePlaceNotBeingRemovedAsync(section.SheetTableId, section.ParentSheetSectionId, currentUser.UserId, cancellationToken);
        var state = await drafts.LoadAsync(sectionId, cancellationToken);
        var siblingOrders = (await liveSections.VisibleAsync(section.SheetTableId, cancellationToken))
            .Where(candidate => candidate.ParentSheetSectionId == section.ParentSheetSectionId && candidate.Id != sectionId)
            .Select(candidate => candidate.DisplayOrder)
            .Order()
            .ToList();
        var order = OrderGaps.PlaceAt(siblingOrders, request.DisplayOrder, state.Current?.DisplayOrder);

        var (draft, _) = await drafts.EnsureMineAsync(sectionId, state, cancellationToken);
        draft.DisplayOrder = order;

        await unitOfWork.SaveSheetChangesAsync(cancellationToken);
        await drafts.ReleaseIfUnchangedAsync(sectionId, cancellationToken);
        await drafts.ReleaseRestoredOrderAsync(
            await sections.ListSiblingIdsAsync(section.SheetTableId, section.ParentSheetSectionId, cancellationToken),
            cancellationToken);
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
        await removals.EnsurePlaceNotBeingRemovedAsync(section.SheetTableId, section.ParentSheetSectionId, currentUser.UserId, cancellationToken);
        await contents.EnsureSectionsFreeAsync(
            await liveSections.SubtreeIdsAsync(section.SheetTableId, section.Id, cancellationToken),
            cancellationToken);

        var sheetId = section.SheetTable.SheetId;
        if (state.IsNew)
        {
            await DeleteSubtreeAsync(section, cancellationToken);
        }
        else
        {
            var (draft, _) = await drafts.EnsureMineAsync(sectionId, state, cancellationToken);
            draft.IsDeleted = true;
            await unitOfWork.SaveSheetChangesAsync(cancellationToken);
        }

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private async Task<SheetSection> LoadSectionAsync(int sectionId, CancellationToken cancellationToken)
    {
        return await sections.FindWithTemplateAndTableAsync(sectionId, cancellationToken)
            ?? throw new NotFoundException($"Section {sectionId} was not found.");
    }

    /// <summary>
    /// Deletes a section only its author ever saw, along with its sub-sections. A section can't cascade
    /// to its own children in SQL Server, so they're deleted deepest first.
    /// </summary>
    private async Task DeleteSubtreeAsync(SheetSection section, CancellationToken cancellationToken)
    {
        var ids = await liveSections.SubtreeIdsAsync(section.SheetTableId, section.Id, cancellationToken);
        var remaining = await sections.GetParentsAsync(ids, cancellationToken);
        while (remaining.Count > 0)
        {
            var leaves = remaining.Keys
                .Where(id => !remaining.ContainsValue(id))
                .ToList();
            await sections.DeleteAsync(leaves, cancellationToken);
            foreach (var leaf in leaves)
            {
                remaining.Remove(leaf);
            }
        }
    }

    /// <summary>A section goes under a copy of its template's parent, or at the top level when it has none.</summary>
    private async Task EnsureParentFitsAsync(int tableId, TemplateSection template, int? parentSheetSectionId, CancellationToken cancellationToken)
    {
        if (parentSheetSectionId is { } parentId)
        {
            var parent = await sections.FindOnTableAsync(parentId, tableId, cancellationToken)
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
    }

    /// <summary>The display order for a new copy after its siblings, refused once the template's maximum is reached.</summary>
    private async Task<int> NextOrderWithinLimitAsync(int tableId, TemplateSection template, int? parentSheetSectionId, CancellationToken cancellationToken)
    {
        var siblings = (await liveSections.VisibleAsync(tableId, cancellationToken))
            .Where(section => section.ParentSheetSectionId == parentSheetSectionId)
            .ToList();
        var copies = siblings.Count(section => section.TemplateSectionId == template.Id);
        if (template.MaxInstances is { } max && copies >= max)
        {
            throw new ConflictException($"'{template.Name}' can have at most {max}.");
        }

        return OrderGaps.Next(siblings.Select(section => section.DisplayOrder));
    }
}
