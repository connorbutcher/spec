using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Drafts;
using PUSpecSheet.DataAccess.Sheets.Layout;
using PUSpecSheet.DataAccess.Sheets.Reading;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.DataAccess.Templates;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Editing;

public sealed class SheetColumnBlocksService : ISheetColumnBlocksService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly ColumnBlockDrafts _drafts;
    private readonly SheetInstantiator _instantiator;
    private readonly ISheetCellFiller _filler;
    private readonly SheetReader _reader;
    private readonly IRemovalInProgressGuard _removals;
    private readonly ICurrentUser _currentUser;
    private readonly ITemplateColumnBlockDataService _templateColumnBlockData;
    private readonly ISheetTableDataService _sheetTableData;
    private readonly ISheetColumnBlockDataService _sheetColumnBlockData;

    public SheetColumnBlocksService(
        PuSpecSheetDbContext db,
        ColumnBlockDrafts drafts,
        SheetInstantiator instantiator,
        ISheetCellFiller filler,
        SheetReader reader,
        IRemovalInProgressGuard removals,
        ICurrentUser currentUser,
        ITemplateColumnBlockDataService templateColumnBlockData,
        ISheetTableDataService sheetTableData,
        ISheetColumnBlockDataService sheetColumnBlockData)
    {
        _db = db;
        _drafts = drafts;
        _instantiator = instantiator;
        _filler = filler;
        _reader = reader;
        _removals = removals;
        _currentUser = currentUser;
        _templateColumnBlockData = templateColumnBlockData;
        _sheetTableData = sheetTableData;
        _sheetColumnBlockData = sheetColumnBlockData;
    }

    public async Task<Sheet> AddAsync(int tableId, AddSheetColumnBlockRequest request, CancellationToken cancellationToken)
    {
        var table = await _sheetTableData.FindWithTemplateVersionAsync(tableId, cancellationToken)
            ?? throw new NotFoundException($"Table {tableId} was not found.");
        if (table.TableTemplateVersion.Orientation != TemplateOrientation.Horizontal)
        {
            throw new InvalidRequestException("Only horizontal tables have column blocks.");
        }

        var template = await _templateColumnBlockData.FindInVersionAsync(request.TemplateColumnBlockId, table.TableTemplateVersionId, cancellationToken)
            ?? throw new InvalidRequestException("That column block isn't part of this table's template.");
        await _removals.EnsurePlaceNotBeingRemovedAsync(tableId, null, _currentUser.UserId, cancellationToken);

        var visible = await VisibleAsync(tableId, cancellationToken);
        var copies = visible.Count(block => block.TemplateColumnBlockId == template.Id);
        if (template.MaxInstances is { } max && copies >= max)
        {
            throw new ConflictException($"'{template.Name}' can have at most {max}.");
        }

        var block = _instantiator.NewColumnBlock(table.Id, table.SheetId, template, OrderGaps.Next(visible.Select(candidate => candidate.DisplayOrder)));
        _db.SheetColumnBlocks.Add(block);
        await _db.SaveSheetChangesAsync(cancellationToken);

        await _filler.FillAsync(tableId, cancellationToken);
        return await _reader.ReadLiveAsync(table.SheetId, cancellationToken);
    }

    public async Task<Sheet> MoveAsync(int columnBlockId, MoveRequest request, CancellationToken cancellationToken)
    {
        var block = await LoadAsync(columnBlockId, cancellationToken);
        await _removals.EnsurePlaceNotBeingRemovedAsync(block.SheetTableId, null, _currentUser.UserId, cancellationToken);
        var state = await _drafts.LoadAsync(columnBlockId, cancellationToken);

        var siblingOrders = (await VisibleAsync(block.SheetTableId, cancellationToken))
            .Where(candidate => candidate.Id != columnBlockId)
            .Select(candidate => candidate.DisplayOrder)
            .Order()
            .ToList();
        var order = OrderGaps.PlaceAt(siblingOrders, request.DisplayOrder, state.Current?.DisplayOrder);

        var (draft, _) = await _drafts.EnsureMineAsync(columnBlockId, state, cancellationToken);
        draft.DisplayOrder = order;

        await _db.SaveSheetChangesAsync(cancellationToken);
        await _drafts.ReleaseIfUnchangedAsync(columnBlockId, cancellationToken);
        await _drafts.ReleaseRestoredOrderAsync(
            await _sheetColumnBlockData.ListIdsOnTableAsync(block.SheetTableId, cancellationToken),
            cancellationToken);
        return await _reader.ReadLiveAsync(block.SheetTable.SheetId, cancellationToken);
    }

    public async Task<Sheet> RemoveAsync(int columnBlockId, CancellationToken cancellationToken)
    {
        var block = await LoadAsync(columnBlockId, cancellationToken);
        var template = block.TemplateColumnBlock;

        var copies = (await VisibleAsync(block.SheetTableId, cancellationToken))
            .Count(candidate => candidate.TemplateColumnBlockId == block.TemplateColumnBlockId);
        if (copies <= template.MinInstances)
        {
            throw new ConflictException($"'{template.Name}' needs at least {template.MinInstances}.");
        }

        var state = await _drafts.LoadAsync(columnBlockId, cancellationToken);
        await _drafts.EnsureNotLockedByOthersAsync(state, cancellationToken);
        await _removals.EnsurePlaceNotBeingRemovedAsync(block.SheetTableId, null, _currentUser.UserId, cancellationToken);

        var sheetId = block.SheetTable.SheetId;
        if (state.IsNew)
        {
            // Only its author ever saw it, so there's no history to keep.
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            await _sheetColumnBlockData.DeleteAsync([columnBlockId], cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        else
        {
            var (draft, _) = await _drafts.EnsureMineAsync(columnBlockId, state, cancellationToken);
            draft.IsDeleted = true;
            await _db.SaveSheetChangesAsync(cancellationToken);
        }

        return await _reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private async Task<DbSheetColumnBlock> LoadAsync(int columnBlockId, CancellationToken cancellationToken)
    {
        return await _sheetColumnBlockData.FindWithTemplateAndTableAsync(columnBlockId, cancellationToken)
            ?? throw new NotFoundException($"Column block {columnBlockId} was not found.");
    }

    private async Task<IReadOnlyList<(int Id, int TemplateColumnBlockId, int DisplayOrder)>> VisibleAsync(int tableId, CancellationToken cancellationToken)
    {
        var me = _currentUser.UserId;
        var blocks = await _sheetColumnBlockData.ListLinksOnTableAsync(tableId, cancellationToken);
        var revisions = await _sheetColumnBlockData.ListVisibleOnTableAsync(tableId, me, cancellationToken);
        var resolved = RevisionResolver.Resolve(revisions, revision => revision.SheetColumnBlockId, me);

        return blocks
            .Where(block => RevisionResolver.IsVisible(resolved.GetValueOrDefault(block.Id)))
            .Select(block => (block.Id, block.TemplateColumnBlockId, resolved[block.Id].Shown!.DisplayOrder))
            .ToList();
    }
}
