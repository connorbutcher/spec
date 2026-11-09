using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Removal;

/// <summary>
/// Stops a section or table being removed while someone else holds something inside it, or anything inside
/// is waiting for approval (see <see cref="HeldContents"/>). Both kinds of hold count: a saved draft, and a
/// row someone has only clicked into.
/// </summary>
public sealed class RemovalContentsGuard(
    PuSpecSheetDbContext db,
    LiveRowCheckoutTracker liveCheckouts,
    ICurrentUser currentUser)
{
    /// <summary>Refuses removing the sections given (a section and everything inside it) while they are held.</summary>
    /// <exception cref="ConflictException">Someone else holds something inside, or it is waiting for approval.</exception>
    public async Task EnsureSectionsFreeAsync(IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken)
    {
        var holds = new List<ContainedHold>();
        holds.AddRange(await DraftHoldsAsync(db.SheetSectionRevisions.Where(revision => sectionIds.Contains(revision.SheetSectionId)), cancellationToken));
        holds.AddRange(await DraftHoldsAsync(db.SheetRowRevisions.Where(revision => sectionIds.Contains(revision.SheetRow.SheetSectionId)), cancellationToken));
        holds.AddRange(await LiveHoldsAsync(db.SheetRows.Where(row => sectionIds.Contains(row.SheetSectionId)), cancellationToken));
        await ThrowIfHeldAsync("this section", holds, cancellationToken);
    }

    /// <summary>Refuses removing a table while anything in it is held.</summary>
    /// <exception cref="ConflictException">Someone else holds something inside, or it is waiting for approval.</exception>
    public async Task EnsureTableFreeAsync(int tableId, CancellationToken cancellationToken)
    {
        var holds = new List<ContainedHold>();
        holds.AddRange(await DraftHoldsAsync(db.SheetSectionRevisions.Where(revision => revision.SheetSection.SheetTableId == tableId), cancellationToken));
        holds.AddRange(await DraftHoldsAsync(db.SheetRowRevisions.Where(revision => revision.SheetRow.SheetSection.SheetTableId == tableId), cancellationToken));
        holds.AddRange(await DraftHoldsAsync(db.SheetColumnBlockRevisions.Where(revision => revision.SheetColumnBlock.SheetTableId == tableId), cancellationToken));
        holds.AddRange(await LiveHoldsAsync(db.SheetRows.Where(row => row.SheetSection.SheetTableId == tableId), cancellationToken));
        await ThrowIfHeldAsync("this table", holds, cancellationToken);
    }

    /// <summary>The drafts among <paramref name="revisions"/> that could stop a removal: other people's, or any waiting for approval.</summary>
    private async Task<List<ContainedHold>> DraftHoldsAsync<TRevision>(IQueryable<TRevision> revisions, CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        var me = currentUser.UserId;
        return await revisions
            .Where(revision => revision.Status == RevisionStatus.Draft && (revision.AuthorUserId != me || revision.SheetSubmissionId != null))
            .Select(revision => new ContainedHold(revision.AuthorUserId, revision.SheetSubmissionId != null))
            .ToListAsync(cancellationToken);
    }

    /// <summary>The rows among <paramref name="rows"/> that someone has clicked into, which only memory knows about.</summary>
    private async Task<List<ContainedHold>> LiveHoldsAsync(IQueryable<SheetRow> rows, CancellationToken cancellationToken)
    {
        var rowIds = await rows.Select(row => row.Id).ToListAsync(cancellationToken);
        return rowIds
            .Select(liveCheckouts.HolderOf)
            .OfType<LiveRowCheckout>()
            .Select(holder => new ContainedHold(holder.UserId, AwaitingApproval: false))
            .ToList();
    }

    private async Task ThrowIfHeldAsync(string subject, List<ContainedHold> holds, CancellationToken cancellationToken)
    {
        var userIds = holds.Select(hold => hold.UserId).Distinct().ToList();
        if (userIds.Count == 0)
        {
            return;
        }

        var names = await db.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
        if (HeldContents.RefusalFor(subject, holds, currentUser.UserId, names) is { } refusal)
        {
            throw new ConflictException(refusal);
        }
    }
}
