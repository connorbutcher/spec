using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Sheets.Collaboration;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public sealed class RowTakeoverService(
    PuSpecSheetDbContext db,
    ICurrentUser currentUser,
    RowTakeoverStore store,
    SheetPresenceTracker presence,
    ISheetLiveNotifier notifier,
    RowTakeoverOptions options,
    TimeProvider clock) : IRowTakeoverService
{
    private const string NoLongerOpen = "That request is no longer open.";

    public async Task<RowTakeoverDto> RequestAsync(int rowId, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var checkout = await db.SheetRowRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetRowId == rowId && revision.Status == RevisionStatus.Draft)
            .Select(revision => new { revision.AuthorUserId, revision.SheetRow.SheetSection.SheetTable.SheetId })
            .SingleOrDefaultAsync(cancellationToken);
        if (checkout is null)
        {
            if (!await db.SheetRows.AnyAsync(row => row.Id == rowId, cancellationToken))
            {
                throw new NotFoundException($"Row {rowId} was not found.");
            }

            throw new ConflictException("This row is no longer checked out, so you can edit it straight away.");
        }

        if (checkout.AuthorUserId == me)
        {
            throw new InvalidRequestException("This row is already checked out to you.");
        }

        if (store.ForRow(rowId) is { } waiting)
        {
            return waiting.RequesterUserId == me
                ? waiting
                : throw new ConflictException($"{waiting.RequesterName} has already asked to take over this row.");
        }

        var names = await db.Users
            .Where(user => user.Id == me || user.Id == checkout.AuthorUserId)
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
        var now = clock.GetUtcNow().UtcDateTime;
        var takeover = new RowTakeoverDto(
            Guid.NewGuid(),
            checkout.SheetId,
            rowId,
            me,
            names.GetValueOrDefault(me, "Another user"),
            checkout.AuthorUserId,
            names.GetValueOrDefault(checkout.AuthorUserId, "Another user"),
            now,
            now + options.ResponseTime,
            RowTakeoverStatus.Pending);

        // Nobody is there to ask, and waiting a minute for silence helps no one.
        if (!presence.IsPresent(takeover.SheetId, takeover.HolderUserId))
        {
            return await GrantAsync(takeover, RowTakeoverStatus.GrantedHolderAway, cancellationToken);
        }

        if (!store.TryAdd(takeover))
        {
            throw new ConflictException("Someone else has just asked to take over this row.");
        }

        await notifier.TakeoverChangedAsync(takeover, cancellationToken);
        return takeover;
    }

    public async Task<RowTakeoverDto> ApproveAsync(Guid takeoverId, CancellationToken cancellationToken)
    {
        var takeover = TakeForHolder(takeoverId);
        return await GrantAsync(takeover, RowTakeoverStatus.Approved, cancellationToken);
    }

    public async Task<RowTakeoverDto> DenyAsync(Guid takeoverId, CancellationToken cancellationToken)
    {
        var takeover = TakeForHolder(takeoverId);
        return await CloseAsync(takeover, RowTakeoverStatus.Denied, cancellationToken);
    }

    public async Task<RowTakeoverDto> CancelAsync(Guid takeoverId, CancellationToken cancellationToken)
    {
        var takeover = store.Find(takeoverId) ?? throw new NotFoundException(NoLongerOpen);
        if (takeover.RequesterUserId != currentUser.UserId)
        {
            throw new InvalidRequestException("Only the person who asked can withdraw this request.");
        }

        if (!store.TryRemove(takeover))
        {
            throw new NotFoundException(NoLongerOpen);
        }

        return await CloseAsync(takeover, RowTakeoverStatus.Cancelled, cancellationToken);
    }

    public async Task GrantOverdueAsync(CancellationToken cancellationToken)
    {
        foreach (var takeover in store.Due(clock.GetUtcNow().UtcDateTime))
        {
            if (store.TryRemove(takeover))
            {
                await GrantAsync(takeover, RowTakeoverStatus.GrantedOnTimeout, cancellationToken);
            }
        }
    }

    public async Task ReleaseSettledAsync(int sheetId, CancellationToken cancellationToken)
    {
        var waiting = store.OnSheet(sheetId);
        if (waiting.Count == 0)
        {
            return;
        }

        var rowIds = waiting.Select(takeover => takeover.RowId).ToList();
        var holders = await db.SheetRowRevisions
            .AsNoTracking()
            .Where(revision => rowIds.Contains(revision.SheetRowId) && revision.Status == RevisionStatus.Draft)
            .ToDictionaryAsync(revision => revision.SheetRowId, revision => revision.AuthorUserId, cancellationToken);

        foreach (var takeover in waiting)
        {
            var stillHeld = holders.TryGetValue(takeover.RowId, out var holder) && holder == takeover.HolderUserId;
            if (!stillHeld && store.TryRemove(takeover))
            {
                await CloseAsync(takeover, RowTakeoverStatus.Released, cancellationToken);
            }
        }
    }

    /// <summary>Takes a waiting request out for the current user to answer, who must be its holder.</summary>
    private RowTakeoverDto TakeForHolder(Guid takeoverId)
    {
        var takeover = store.Find(takeoverId) ?? throw new NotFoundException(NoLongerOpen);
        if (takeover.HolderUserId != currentUser.UserId)
        {
            throw new InvalidRequestException("Only the person the row is checked out to can answer this request.");
        }

        return store.TryRemove(takeover) ? takeover : throw new NotFoundException(NoLongerOpen);
    }

    /// <summary>
    /// Moves the row's draft from the holder to the requester, as it stands. If the holder no longer holds
    /// it there is nothing to hand over, and the request closes as released.
    /// </summary>
    private async Task<RowTakeoverDto> GrantAsync(RowTakeoverDto takeover, RowTakeoverStatus status, CancellationToken cancellationToken)
    {
        var draft = await db.SheetRowRevisions
            .SingleOrDefaultAsync(
                revision => revision.SheetRowId == takeover.RowId && revision.Status == RevisionStatus.Draft,
                cancellationToken);
        if (draft is null || draft.AuthorUserId != takeover.HolderUserId)
        {
            return await CloseAsync(takeover, RowTakeoverStatus.Released, cancellationToken);
        }

        draft.AuthorUserId = takeover.RequesterUserId;
        draft.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveSheetChangesAsync(cancellationToken);

        var granted = await CloseAsync(takeover, status, cancellationToken);
        await notifier.SheetChangedAsync(takeover.SheetId, exceptConnectionId: null, cancellationToken);
        return granted;
    }

    private async Task<RowTakeoverDto> CloseAsync(RowTakeoverDto takeover, RowTakeoverStatus status, CancellationToken cancellationToken)
    {
        var closed = takeover with { Status = status };
        await notifier.TakeoverChangedAsync(closed, cancellationToken);
        return closed;
    }
}
