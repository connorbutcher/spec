using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Layout;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Editing;

public sealed class SheetColumnBlockService(
    ISheetTableRepository tables,
    ISheetColumnBlockRepository columnBlocks,
    ISheetColumnBlockRevisionRepository columnBlockRevisions,
    ITemplateColumnBlockRepository templateColumnBlocks,
    IUnitOfWork unitOfWork,
    ColumnBlockDrafts drafts,
    SheetInstantiator instantiator,
    ISheetCellFiller filler,
    SheetReader reader,
    IRemovalInProgressGuard removals,
    ICurrentUser currentUser) : ISheetColumnBlockService
{
    public async Task<SheetDto> AddAsync(int tableId, AddSheetColumnBlockRequest request, CancellationToken cancellationToken)
    {
        var table = await tables.FindWithTemplateVersionAsync(tableId, cancellationToken)
            ?? throw new NotFoundException($"Table {tableId} was not found.");
        if (table.TableTemplateVersion.Orientation != TemplateOrientation.Horizontal)
        {
            throw new InvalidRequestException("Only horizontal tables have column blocks.");
        }

        var template = await templateColumnBlocks.FindInVersionAsync(request.TemplateColumnBlockId, table.TableTemplateVersionId, cancellationToken)
            ?? throw new InvalidRequestException("That column block isn't part of this table's template.");
        await removals.EnsurePlaceNotBeingRemovedAsync(tableId, null, currentUser.UserId, cancellationToken);

        var visible = await VisibleAsync(tableId, cancellationToken);
        var copies = visible.Count(block => block.TemplateColumnBlockId == template.Id);
        if (template.MaxInstances is { } max && copies >= max)
        {
            throw new ConflictException($"'{template.Name}' can have at most {max}.");
        }

        var block = instantiator.NewColumnBlock(table.Id, table.SheetId, template, OrderGaps.Next(visible.Select(candidate => candidate.DisplayOrder)));
        columnBlocks.Add(block);
        await unitOfWork.SaveSheetChangesAsync(cancellationToken);

        await filler.FillAsync(tableId, cancellationToken);
        return await reader.ReadLiveAsync(table.SheetId, cancellationToken);
    }

    public async Task<SheetDto> MoveAsync(int columnBlockId, MoveRequest request, CancellationToken cancellationToken)
    {
        var block = await LoadAsync(columnBlockId, cancellationToken);
        await removals.EnsurePlaceNotBeingRemovedAsync(block.SheetTableId, null, currentUser.UserId, cancellationToken);
        var state = await drafts.LoadAsync(columnBlockId, cancellationToken);

        var siblingOrders = (await VisibleAsync(block.SheetTableId, cancellationToken))
            .Where(candidate => candidate.Id != columnBlockId)
            .Select(candidate => candidate.DisplayOrder)
            .Order()
            .ToList();
        var order = OrderGaps.PlaceAt(siblingOrders, request.DisplayOrder, state.Current?.DisplayOrder);

        var (draft, _) = await drafts.EnsureMineAsync(columnBlockId, state, cancellationToken);
        draft.DisplayOrder = order;

        await unitOfWork.SaveSheetChangesAsync(cancellationToken);
        await drafts.ReleaseIfUnchangedAsync(columnBlockId, cancellationToken);
        await drafts.ReleaseRestoredOrderAsync(
            await columnBlocks.ListIdsOnTableAsync(block.SheetTableId, cancellationToken),
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
        await removals.EnsurePlaceNotBeingRemovedAsync(block.SheetTableId, null, currentUser.UserId, cancellationToken);

        var sheetId = block.SheetTable.SheetId;
        if (state.IsNew)
        {
            // Only its author ever saw it, so there's no history to keep.
            await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            await columnBlocks.DeleteAsync([columnBlockId], cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        else
        {
            var (draft, _) = await drafts.EnsureMineAsync(columnBlockId, state, cancellationToken);
            draft.IsDeleted = true;
            await unitOfWork.SaveSheetChangesAsync(cancellationToken);
        }

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private async Task<SheetColumnBlock> LoadAsync(int columnBlockId, CancellationToken cancellationToken)
    {
        return await columnBlocks.FindWithTemplateAndTableAsync(columnBlockId, cancellationToken)
            ?? throw new NotFoundException($"Column block {columnBlockId} was not found.");
    }

    /// <summary>The table's column block copies that exist for the user: published and not removed, or drafted by them.</summary>
    private async Task<IReadOnlyList<(int Id, int TemplateColumnBlockId, int DisplayOrder)>> VisibleAsync(int tableId, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var blocks = await columnBlocks.ListLinksOnTableAsync(tableId, cancellationToken);
        var revisions = await columnBlockRevisions.ListVisibleOnTableAsync(tableId, me, cancellationToken);
        var resolved = RevisionResolver.Resolve(revisions, revision => revision.SheetColumnBlockId, me);

        return blocks
            .Where(block => RevisionResolver.IsVisible(resolved.GetValueOrDefault(block.Id)))
            .Select(block => (block.Id, block.TemplateColumnBlockId, resolved[block.Id].Shown!.DisplayOrder))
            .ToList();
    }
}
