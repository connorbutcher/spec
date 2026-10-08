using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Application.Sheets.Linking;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets;

public sealed class SheetRowService(
    PuSpecSheetDbContext db,
    RowDrafts drafts,
    LiveRowCheckoutGuard liveCheckouts,
    RowDraftStarter draftStarter,
    RowValueStore valueStore,
    LinkedDropdownValueRule linkedValues,
    NewRowSettings newRowSettings,
    SheetInstantiator instantiator,
    ISheetCellFiller filler,
    SheetReader reader,
    ICurrentUser currentUser) : ISheetRowService
{
    public async Task<SheetDto> AddAsync(int sectionId, AddSheetRowRequest request, CancellationToken cancellationToken)
    {
        var section = await db.SheetSections
            .AsNoTracking()
            .Include(candidate => candidate.TemplateSection)
            .Include(candidate => candidate.SheetTable)
            .SingleOrDefaultAsync(candidate => candidate.Id == sectionId, cancellationToken)
            ?? throw new NotFoundException($"Section {sectionId} was not found.");
        if (section.TemplateSection.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("Rows can't be added to the header.");
        }

        var templateRow = await db.TemplateRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .SingleOrDefaultAsync(row => row.Id == request.TemplateRowId && row.TemplateSectionId == section.TemplateSectionId, cancellationToken)
            ?? throw new InvalidRequestException("That row isn't part of this section's template.");

        var orders = await VisibleRowOrdersAsync(sectionId, excludingRowId: null, cancellationToken);
        var row = instantiator.NewRow(templateRow, OrderGaps.Next(orders));
        row.SheetSectionId = sectionId;

        db.SheetRows.Add(row);
        await db.SaveSheetChangesAsync(cancellationToken);
        await filler.FillAsync(section.SheetTableId, cancellationToken);
        await newRowSettings.InheritAsync(section.SheetTableId, [row.Id], cancellationToken);
        return await reader.ReadLiveAsync(section.SheetTable.SheetId, cancellationToken);
    }

    public async Task<SheetDto> LockAsync(int rowId, CancellationToken cancellationToken)
    {
        var sheetId = await SheetIdOfAsync(rowId, cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await draftStarter.StartAsync(rowId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> SaveValuesAsync(int rowId, SaveRowValuesRequest request, CancellationToken cancellationToken)
    {
        var row = await db.SheetRows
            .AsNoTracking()
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.SheetTable)
            .Include(candidate => candidate.Cells)
            .ThenInclude(cell => cell.TemplateCell)
            .ThenInclude(templateCell => templateCell.CellType)
            .ThenInclude(cellType => cellType.Options)
            .AsSplitQuery()
            .SingleOrDefaultAsync(candidate => candidate.Id == rowId, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");

        // Check every value before locking the row or changing anything.
        var cellsById = row.Cells.ToDictionary(cell => cell.Id);
        foreach (var value in request.Values)
        {
            if (!cellsById.TryGetValue(value.SheetCellId, out var cell))
            {
                throw new InvalidRequestException($"Cell {value.SheetCellId} isn't in this row.");
            }

            CellValueValidator.Validate(cell.TemplateCell, value);
        }

        await linkedValues.EnsureChoicesAsync(row.SheetSection.SheetTable.SheetId, cellsById, request.Values, cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var draft = await draftStarter.StartAsync(rowId, cancellationToken);
        var changes = request.Values
            .Select(value => new CellValueChange(
                value.SheetCellId,
                cellsById[value.SheetCellId].TemplateCell.CellType.Kind,
                value with { Text = value.Text?.Trim() }))
            .ToList();
        await valueStore.SetManyAsync(draft.Id, changes, cancellationToken);
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
            await db.SheetRows.Where(candidate => candidate.SheetSectionId == sectionId).Select(candidate => candidate.Id).ToListAsync(cancellationToken),
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> RemoveAsync(int rowId, CancellationToken cancellationToken)
    {
        var row = await db.SheetRows
            .AsNoTracking()
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.TemplateSection)
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.SheetTable)
            .SingleOrDefaultAsync(candidate => candidate.Id == rowId, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
        if (row.SheetSection.TemplateSection.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("Rows can't be removed from the header.");
        }

        var sheetId = row.SheetSection.SheetTable.SheetId;
        liveCheckouts.EnsureNotHeldByOthers(rowId);
        var state = await drafts.LoadAsync(rowId, cancellationToken);
        await drafts.EnsureNotLockedByOthersAsync(state, cancellationToken);

        if (state.IsNew)
        {
            // Only its author ever saw it, so there's no history to keep.
            await db.SheetRows.Where(candidate => candidate.Id == rowId).ExecuteDeleteAsync(cancellationToken);
        }
        else
        {
            var (draft, _) = await drafts.EnsureMineAsync(rowId, state, cancellationToken);
            draft.IsDeleted = true;
            await db.SaveSheetChangesAsync(cancellationToken);
        }

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> DiscardAsync(int rowId, CancellationToken cancellationToken)
    {
        var sheetId = await SheetIdOfAsync(rowId, cancellationToken);
        var state = await drafts.LoadAsync(rowId, cancellationToken);
        if (state.Draft is { } draft)
        {
            await drafts.EnsureNotLockedByOthersAsync(state, cancellationToken);
            if (state.IsNew)
            {
                await db.SheetRows.Where(candidate => candidate.Id == rowId).ExecuteDeleteAsync(cancellationToken);
            }
            else
            {
                db.SheetRowRevisions.Remove(draft);
                await db.SaveSheetChangesAsync(cancellationToken);
            }
        }

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private async Task<int> SheetIdOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.Id == rowId)
            .Select(row => (int?)row.SheetSection.SheetTable.SheetId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
    }

    private async Task<int> SectionIdOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.Id == rowId)
            .Select(row => (int?)row.SheetSectionId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
    }

    /// <summary>The display orders of the section's rows the user can see, ascending, optionally leaving one out.</summary>
    private async Task<IReadOnlyList<int>> VisibleRowOrdersAsync(int sectionId, int? excludingRowId, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var revisions = await db.SheetRowRevisions
            .Where(revision => revision.SheetRow.SheetSectionId == sectionId)
            .VisibleTo(me)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return RevisionResolver.Resolve(revisions, revision => revision.SheetRowId, me)
            .Where(entry => entry.Key != excludingRowId && RevisionResolver.IsVisible(entry.Value))
            .Select(entry => entry.Value.Shown!.DisplayOrder)
            .Order()
            .ToList();
    }
}
