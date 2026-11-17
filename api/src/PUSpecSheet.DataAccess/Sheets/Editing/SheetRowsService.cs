using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;
using PUSpecSheet.DataAccess.Sheets.Drafts;
using PUSpecSheet.DataAccess.Sheets.Layout;
using PUSpecSheet.DataAccess.Sheets.Linking;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Reading;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.DataAccess.Sheets.Values;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Editing;

public sealed class SheetRowsService : ISheetRowsService
{
    private readonly PuSpecSheetDbContext db;
    private readonly RowDrafts drafts;
    private readonly LiveRowCheckoutGuard liveCheckouts;
    private readonly RowDraftStarter draftStarter;
    private readonly LinkedDropdownValueRule linkedValues;
    private readonly SheetReader reader;
    private readonly IRemovalInProgressGuard removals;
    private readonly ICurrentUser currentUser;
    private readonly ISheetCellSettingsService cellSettings;

    public SheetRowsService(
        PuSpecSheetDbContext db,
        RowDrafts drafts,
        LiveRowCheckoutGuard liveCheckouts,
        RowDraftStarter draftStarter,
        LinkedDropdownValueRule linkedValues,
        SheetReader reader,
        IRemovalInProgressGuard removals,
        ICurrentUser currentUser,
        ISheetCellSettingsService cellSettings)
    {
        this.db = db;
        this.drafts = drafts;
        this.liveCheckouts = liveCheckouts;
        this.draftStarter = draftStarter;
        this.linkedValues = linkedValues;
        this.reader = reader;
        this.removals = removals;
        this.currentUser = currentUser;
        this.cellSettings = cellSettings;
    }

    public async Task<SheetDto> SaveValuesAsync(int rowId, SaveRowValuesRequest request, CancellationToken cancellationToken)
    {
        var row = await LoadRowWithCellTypesAsync(rowId, cancellationToken);

        // Check every value before locking the row or changing anything.
        var cellsById = row.Cells.ToDictionary(cell => cell.Id);
        var changes = ReadValues(request, cellsById);
        await linkedValues.EnsureChoicesAsync(row.SheetSection.SheetTable.SheetId, cellsById, changes, cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var draft = await draftStarter.StartAsync(rowId, cancellationToken);
        foreach (var change in changes)
        {
            change.Value.Text = change.Value.Text?.Trim();
            change.Value.Slots = SlotValues.Clean(change.Value.Slots);
        }

        await RowValueQueries.SetManyAsync(db, draft.Id, changes, cancellationToken);
        await db.SaveSheetChangesAsync(cancellationToken);

        // Values put back to what is published leave nothing to publish, so the row is released.
        await drafts.ReleaseIfUnchangedAsync(rowId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await reader.ReadLiveAsync(row.SheetSection.SheetTable.SheetId, cancellationToken);
    }

    public async Task<SheetDto> MoveAsync(int rowId, MoveRequest request, CancellationToken cancellationToken)
    {
        var sectionId = await SectionIdOfAsync(rowId, cancellationToken);
        var sheetId = await SheetIdOfAsync(rowId, cancellationToken);

        var siblingOrders = await VisibleRowOrdersAsync(sectionId, rowId, cancellationToken);
        var published = (await drafts.LoadAsync(rowId, cancellationToken)).Current?.DisplayOrder;
        var order = OrderGaps.PlaceAt(siblingOrders, request.DisplayOrder, published);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var draft = await draftStarter.StartAsync(rowId, cancellationToken);
        draft.DisplayOrder = order;
        await db.SaveSheetChangesAsync(cancellationToken);
        await drafts.ReleaseIfUnchangedAsync(rowId, cancellationToken);
        await drafts.ReleaseRestoredOrderAsync(
            await SheetRowQueries.ListIdsInSectionAsync(db, sectionId, cancellationToken),
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> RemoveAsync(int rowId, CancellationToken cancellationToken)
    {
        var row = await SheetRowQueries.FindWithSectionAsync(db, rowId, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
        if (row.SheetSection.TemplateSection.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("Rows can't be removed from the header.");
        }

        var sheetId = row.SheetSection.SheetTable.SheetId;
        liveCheckouts.EnsureNotHeldByOthers(rowId);
        var state = await drafts.LoadAsync(rowId, cancellationToken);
        await drafts.EnsureNotLockedByOthersAsync(state, cancellationToken);
        await removals.EnsurePlaceNotBeingRemovedAsync(row.SheetSection.SheetTableId, row.SheetSectionId, currentUser.UserId, cancellationToken);

        if (state.IsNew)
        {
            // Only its author ever saw it, so there's no history to keep.
            await SheetRowQueries.DeleteAsync(db, rowId, cancellationToken);
        }
        else
        {
            var (draft, _) = await drafts.EnsureMineAsync(rowId, state, cancellationToken);
            draft.IsDeleted = true;
            await db.SaveSheetChangesAsync(cancellationToken);
        }

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public Task<SheetDto> SaveCellSettingsAsync(int rowId, SaveRowCellSettingsRequest request, CancellationToken cancellationToken)
    {
        return cellSettings.SaveAsync(rowId, request, cancellationToken);
    }

    private async Task<int> SheetIdOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return await SheetRowQueries.FindSheetIdAsync(db, rowId, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
    }

    private async Task<int> SectionIdOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return await SheetRowQueries.FindSectionIdAsync(db, rowId, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
    }

    /// <summary>The display orders of the section's rows the user can see, ascending, optionally leaving one out.</summary>
    private async Task<IReadOnlyList<int>> VisibleRowOrdersAsync(int sectionId, int? excludingRowId, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var revisions = await SheetRowRevisionQueries.ListVisibleInSectionAsync(db, sectionId, me, cancellationToken);

        return RevisionResolver.Resolve(revisions, revision => revision.SheetRowId, me)
            .Where(entry => entry.Key != excludingRowId && RevisionResolver.IsVisible(entry.Value))
            .Select(entry => entry.Value.Shown!.DisplayOrder)
            .Order()
            .ToList();
    }

    /// <summary>Reads each value sent in the shape its cell's kind takes, and checks it against the cell.</summary>
    private static List<CellValueChange> ReadValues(SaveRowValuesRequest request, Dictionary<int, DbSheetCell> cellsById)
    {
        var changes = new List<CellValueChange>();
        foreach (var value in request.Values)
        {
            if (!cellsById.TryGetValue(value.SheetCellId, out var cell))
            {
                throw new InvalidRequestException($"Cell {value.SheetCellId} isn't in this row.");
            }

            var template = cell.TemplateCell;
            var name = string.IsNullOrWhiteSpace(template.Caption) ? template.CellType.Name : template.Caption;
            var bag = CellValueMapper.FromRequest(template.CellType.Kind, value.Value, name);
            CellValueValidator.Validate(template, bag);
            changes.Add(new CellValueChange(value.SheetCellId, template.CellType.Kind, bag));
        }

        return changes;
    }

    private async Task<DbSheetRow> LoadRowWithCellTypesAsync(int rowId, CancellationToken cancellationToken)
    {
        return await SheetRowQueries.FindWithCellTypesAsync(db, rowId, withOptions: true, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
    }
}
