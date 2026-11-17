using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Drafts;
using PUSpecSheet.DataAccess.Sheets.Layout;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Reading;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.DataAccess.Sheets.Values;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Sheets.Editing;

public sealed class SheetSectionsService : ISheetSectionsService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly SectionDrafts _drafts;
    private readonly LiveSectionQuery _liveSections;
    private readonly SheetInstantiator _instantiator;
    private readonly ISheetCellFiller _filler;
    private readonly NewRowSettings _newRowSettings;
    private readonly SheetReader _reader;
    private readonly RemovalContentsGuard _contents;
    private readonly IRemovalInProgressGuard _removals;
    private readonly ICurrentUser _currentUser;

    public SheetSectionsService(
        PuSpecSheetDbContext db,
        SectionDrafts drafts,
        LiveSectionQuery liveSections,
        SheetInstantiator instantiator,
        ISheetCellFiller filler,
        NewRowSettings newRowSettings,
        SheetReader reader,
        RemovalContentsGuard contents,
        IRemovalInProgressGuard removals,
        ICurrentUser currentUser)
    {
        _db = db;
        _drafts = drafts;
        _liveSections = liveSections;
        _instantiator = instantiator;
        _filler = filler;
        _newRowSettings = newRowSettings;
        _reader = reader;
        _contents = contents;
        _removals = removals;
        _currentUser = currentUser;
    }

    public async Task<Sheet> AddAsync(int tableId, AddSheetSectionRequest request, CancellationToken cancellationToken)
    {
        var table = await SheetTableQueries.FindForEditAsync(_db, tableId, cancellationToken)
            ?? throw new NotFoundException($"Table {tableId} was not found.");

        var tree = await TemplateTree.LoadAsync(_db, table.TableTemplateVersionId, cancellationToken);
        var template = tree.Find(request.TemplateSectionId)
            ?? throw new InvalidRequestException("That section isn't part of this table's template.");
        if (template.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("The header comes with the table and can't be added again.");
        }

        await EnsureParentFitsAsync(tableId, template, request.ParentSheetSectionId, cancellationToken);
        await _removals.EnsurePlaceNotBeingRemovedAsync(tableId, request.ParentSheetSectionId, _currentUser.UserId, cancellationToken);
        var order = await NextOrderWithinLimitAsync(tableId, template, request.ParentSheetSectionId, cancellationToken);
        var section = _instantiator.NewSection(table, template, tree, order);
        section.ParentSheetSectionId = request.ParentSheetSectionId;

        _db.SheetSections.Add(section);
        await _db.SaveSheetChangesAsync(cancellationToken);
        await _filler.FillAsync(tableId, cancellationToken);
        await _newRowSettings.InheritAsync(section, cancellationToken);
        return await _reader.ReadLiveAsync(table.SheetId, cancellationToken);
    }

    public async Task<Sheet> MoveAsync(int sectionId, MoveRequest request, CancellationToken cancellationToken)
    {
        var section = await LoadSectionAsync(sectionId, cancellationToken);
        if (section.TemplateSection.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("The header always stays first.");
        }

        await _removals.EnsurePlaceNotBeingRemovedAsync(section.SheetTableId, section.ParentSheetSectionId, _currentUser.UserId, cancellationToken);
        var state = await _drafts.LoadAsync(sectionId, cancellationToken);
        var siblingOrders = (await _liveSections.VisibleAsync(section.SheetTableId, cancellationToken))
            .Where(candidate => candidate.ParentSheetSectionId == section.ParentSheetSectionId && candidate.Id != sectionId)
            .Select(candidate => candidate.DisplayOrder)
            .Order()
            .ToList();
        var order = OrderGaps.PlaceAt(siblingOrders, request.DisplayOrder, state.Current?.DisplayOrder);

        var (draft, _) = await _drafts.EnsureMineAsync(sectionId, state, cancellationToken);
        draft.DisplayOrder = order;

        await _db.SaveSheetChangesAsync(cancellationToken);
        await _drafts.ReleaseIfUnchangedAsync(sectionId, cancellationToken);
        await _drafts.ReleaseRestoredOrderAsync(
            await SheetSectionQueries.ListSiblingIdsAsync(_db, section.SheetTableId, section.ParentSheetSectionId, cancellationToken),
            cancellationToken);
        return await _reader.ReadLiveAsync(section.SheetTable.SheetId, cancellationToken);
    }

    public async Task<Sheet> RemoveAsync(int sectionId, CancellationToken cancellationToken)
    {
        var section = await LoadSectionAsync(sectionId, cancellationToken);
        var template = section.TemplateSection;
        if (template.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("The header can't be removed.");
        }

        var siblings = (await _liveSections.VisibleAsync(section.SheetTableId, cancellationToken))
            .Where(candidate => candidate.ParentSheetSectionId == section.ParentSheetSectionId
                && candidate.TemplateSectionId == section.TemplateSectionId)
            .ToList();
        if (siblings.Count <= template.MinInstances)
        {
            throw new ConflictException($"'{template.Name}' needs at least {template.MinInstances}.");
        }

        var state = await _drafts.LoadAsync(sectionId, cancellationToken);
        await _drafts.EnsureNotLockedByOthersAsync(state, cancellationToken);
        await _removals.EnsurePlaceNotBeingRemovedAsync(section.SheetTableId, section.ParentSheetSectionId, _currentUser.UserId, cancellationToken);
        await _contents.EnsureSectionsFreeAsync(
            await _liveSections.SubtreeIdsAsync(section.SheetTableId, section.Id, cancellationToken),
            cancellationToken);

        var sheetId = section.SheetTable.SheetId;
        if (state.IsNew)
        {
            await DeleteSubtreeAsync(section, cancellationToken);
        }
        else
        {
            var (draft, _) = await _drafts.EnsureMineAsync(sectionId, state, cancellationToken);
            draft.IsDeleted = true;
            await _db.SaveSheetChangesAsync(cancellationToken);
        }

        return await _reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private async Task<DbSheetSection> LoadSectionAsync(int sectionId, CancellationToken cancellationToken)
    {
        return await SheetSectionQueries.FindWithTemplateAndTableAsync(_db, sectionId, cancellationToken)
            ?? throw new NotFoundException($"Section {sectionId} was not found.");
    }

    /// <summary>
    /// Deletes a section only its author ever saw, along with its sub-sections. A section can't cascade
    /// to its own children in SQL Server, so they're deleted deepest first.
    /// </summary>
    private async Task DeleteSubtreeAsync(DbSheetSection section, CancellationToken cancellationToken)
    {
        var ids = await _liveSections.SubtreeIdsAsync(section.SheetTableId, section.Id, cancellationToken);
        var remaining = await SheetSectionQueries.GetParentsAsync(_db, ids, cancellationToken);
        while (remaining.Count > 0)
        {
            var leaves = remaining.Keys
                .Where(id => !remaining.ContainsValue(id))
                .ToList();
            await SheetSectionQueries.DeleteAsync(_db, leaves, cancellationToken);
            foreach (var leaf in leaves)
            {
                remaining.Remove(leaf);
            }
        }
    }

    /// <summary>A section goes under a copy of its template's parent, or at the top level when it has none.</summary>
    private async Task EnsureParentFitsAsync(int tableId, DbTemplateSection template, int? parentSheetSectionId, CancellationToken cancellationToken)
    {
        if (parentSheetSectionId is { } parentId)
        {
            var parent = await SheetSectionQueries.FindOnTableAsync(_db, parentId, tableId, cancellationToken)
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
    private async Task<int> NextOrderWithinLimitAsync(int tableId, DbTemplateSection template, int? parentSheetSectionId, CancellationToken cancellationToken)
    {
        var siblings = (await _liveSections.VisibleAsync(tableId, cancellationToken))
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
