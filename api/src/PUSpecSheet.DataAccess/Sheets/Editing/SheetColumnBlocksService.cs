using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Drafts;
using PUSpecSheet.DataAccess.Sheets.Layout;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Reading;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.DataAccess.Templates.Queries;
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

    public SheetColumnBlocksService(
        PuSpecSheetDbContext db,
        ColumnBlockDrafts drafts,
        SheetInstantiator instantiator,
        ISheetCellFiller filler,
        SheetReader reader,
        IRemovalInProgressGuard removals,
        ICurrentUser currentUser)
    {
        _db = db;
        _drafts = drafts;
        _instantiator = instantiator;
        _filler = filler;
        _reader = reader;
        _removals = removals;
        _currentUser = currentUser;
    }

    public async Task<SheetDto> AddAsync(int tableId, AddSheetColumnBlockRequest request, CancellationToken cancellationToken)
    {
        var table = await SheetTableQueries.FindWithTemplateVersionAsync(_db, tableId, cancellationToken)
            ?? throw new NotFoundException($"Table {tableId} was not found.");
        if (table.TableTemplateVersion.Orientation != TemplateOrientation.Horizontal)
        {
            throw new InvalidRequestException("Only horizontal tables have column blocks.");
        }

        var template = await TemplateColumnBlockQueries.FindInVersionAsync(_db, request.TemplateColumnBlockId, table.TableTemplateVersionId, cancellationToken)
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

    public async Task<SheetDto> MoveAsync(int columnBlockId, MoveRequest request, CancellationToken cancellationToken)
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
            await SheetColumnBlockQueries.ListIdsOnTableAsync(_db, block.SheetTableId, cancellationToken),
            cancellationToken);
        return await _reader.ReadLiveAsync(block.SheetTable.SheetId, cancellationToken);
    }

    public async Task<SheetDto> RemoveAsync(int columnBlockId, CancellationToken cancellationToken)
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
            await SheetColumnBlockQueries.DeleteAsync(_db, [columnBlockId], cancellationToken);
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
        return await SheetColumnBlockQueries.FindWithTemplateAndTableAsync(_db, columnBlockId, cancellationToken)
            ?? throw new NotFoundException($"Column block {columnBlockId} was not found.");
    }

    /// <summary>The table's column block copies that exist for the user: published and not removed, or drafted by them.</summary>
    private async Task<IReadOnlyList<(int Id, int TemplateColumnBlockId, int DisplayOrder)>> VisibleAsync(int tableId, CancellationToken cancellationToken)
    {
        var me = _currentUser.UserId;
        var blocks = await SheetColumnBlockQueries.ListLinksOnTableAsync(_db, tableId, cancellationToken);
        var revisions = await SheetColumnBlockRevisionQueries.ListVisibleOnTableAsync(_db, tableId, me, cancellationToken);
        var resolved = RevisionResolver.Resolve(revisions, revision => revision.SheetColumnBlockId, me);

        return blocks
            .Where(block => RevisionResolver.IsVisible(resolved.GetValueOrDefault(block.Id)))
            .Select(block => (block.Id, block.TemplateColumnBlockId, resolved[block.Id].Shown!.DisplayOrder))
            .ToList();
    }
}
