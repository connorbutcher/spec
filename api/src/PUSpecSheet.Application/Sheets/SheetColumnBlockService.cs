using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets;

public sealed class SheetColumnBlockService(
    PuSpecSheetDbContext db,
    ColumnBlockDrafts drafts,
    SheetInstantiator instantiator,
    ISheetCellFiller filler,
    SheetReader reader,
    ICurrentUser currentUser) : ISheetColumnBlockService
{
    public async Task<SheetDto> AddAsync(int tableId, AddSheetColumnBlockRequest request, CancellationToken cancellationToken)
    {
        var table = await db.SheetTables
            .AsNoTracking()
            .Include(candidate => candidate.TableTemplateVersion)
            .SingleOrDefaultAsync(candidate => candidate.Id == tableId, cancellationToken)
            ?? throw new NotFoundException($"Table {tableId} was not found.");
        if (table.TableTemplateVersion.Orientation != TemplateOrientation.Horizontal)
        {
            throw new InvalidRequestException("Only horizontal tables have column blocks.");
        }

        var template = await db.TemplateColumnBlocks
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == request.TemplateColumnBlockId && candidate.TableTemplateVersionId == table.TableTemplateVersionId,
                cancellationToken)
            ?? throw new InvalidRequestException("That column block isn't part of this table's template.");

        var visible = await VisibleAsync(tableId, cancellationToken);
        var copies = visible.Count(block => block.TemplateColumnBlockId == template.Id);
        if (template.MaxInstances is { } max && copies >= max)
        {
            throw new ConflictException($"'{template.Name}' can have at most {max}.");
        }

        var block = instantiator.NewColumnBlock(table.Id, template, OrderGaps.Next(visible.Select(candidate => candidate.DisplayOrder)));
        db.SheetColumnBlocks.Add(block);
        await db.SaveSheetChangesAsync(cancellationToken);

        await filler.FillAsync(tableId, cancellationToken);
        return await reader.ReadLiveAsync(table.SheetId, cancellationToken);
    }

    public async Task<SheetDto> MoveAsync(int columnBlockId, MoveRequest request, CancellationToken cancellationToken)
    {
        var block = await LoadAsync(columnBlockId, cancellationToken);
        var state = await drafts.LoadAsync(columnBlockId, cancellationToken);

        var siblingOrders = (await VisibleAsync(block.SheetTableId, cancellationToken))
            .Where(candidate => candidate.Id != columnBlockId)
            .Select(candidate => candidate.DisplayOrder)
            .Order()
            .ToList();
        var order = OrderGaps.PlaceAt(siblingOrders, request.DisplayOrder, state.Current?.DisplayOrder);

        var (draft, _) = await drafts.EnsureMineAsync(columnBlockId, state, cancellationToken);
        draft.DisplayOrder = order;

        await db.SaveSheetChangesAsync(cancellationToken);
        await drafts.ReleaseIfUnchangedAsync(columnBlockId, cancellationToken);
        await drafts.ReleaseRestoredOrderAsync(
            await db.SheetColumnBlocks.Where(candidate => candidate.SheetTableId == block.SheetTableId).Select(candidate => candidate.Id).ToListAsync(cancellationToken),
            cancellationToken);
        return await reader.ReadLiveAsync(block.SheetTable.SheetId, cancellationToken);
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

        var state = await drafts.LoadAsync(columnBlockId, cancellationToken);
        await drafts.EnsureNotLockedByOthersAsync(state, cancellationToken);

        var sheetId = block.SheetTable.SheetId;
        if (state.IsNew)
        {
            // Only its author ever saw it, so there's no history to keep.
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            await ColumnBlockCells.DeleteBlocksAsync(db, [columnBlockId], cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        else
        {
            var (draft, _) = await drafts.EnsureMineAsync(columnBlockId, state, cancellationToken);
            draft.IsDeleted = true;
            await db.SaveSheetChangesAsync(cancellationToken);
        }

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private async Task<SheetColumnBlock> LoadAsync(int columnBlockId, CancellationToken cancellationToken)
    {
        return await db.SheetColumnBlocks
            .AsNoTracking()
            .Include(block => block.TemplateColumnBlock)
            .Include(block => block.SheetTable)
            .SingleOrDefaultAsync(block => block.Id == columnBlockId, cancellationToken)
            ?? throw new NotFoundException($"Column block {columnBlockId} was not found.");
    }

    /// <summary>The table's column block copies that exist for the user: published and not removed, or drafted by them.</summary>
    private async Task<IReadOnlyList<(int Id, int TemplateColumnBlockId, int DisplayOrder)>> VisibleAsync(int tableId, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var blocks = await db.SheetColumnBlocks
            .AsNoTracking()
            .Where(block => block.SheetTableId == tableId)
            .Select(block => new { block.Id, block.TemplateColumnBlockId })
            .ToListAsync(cancellationToken);
        var revisions = await db.SheetColumnBlockRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetColumnBlock.SheetTableId == tableId
                && revision.SupersededAtUtc == null
                && (revision.Status == RevisionStatus.Published || revision.AuthorUserId == me))
            .ToListAsync(cancellationToken);
        var resolved = RevisionResolver.Resolve(revisions, revision => revision.SheetColumnBlockId, me);

        return blocks
            .Where(block => RevisionResolver.IsVisible(resolved.GetValueOrDefault(block.Id)))
            .Select(block => (block.Id, block.TemplateColumnBlockId, resolved[block.Id].Shown!.DisplayOrder))
            .ToList();
    }
}
