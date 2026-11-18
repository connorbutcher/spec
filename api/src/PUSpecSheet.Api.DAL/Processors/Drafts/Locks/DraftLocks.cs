using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Locks;

/// <summary>
/// The checkout rules for a sheet's drafts. An item's draft is checked out to the user who made it: only they can change
/// it, and nobody can while it is in a submission awaiting approval. Nothing can be added inside an item someone else is
/// adding or removing, and an item can't be removed while someone else has changes inside it.
/// </summary>
internal class DraftLocks
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly User _user;

    public DraftLocks(PUSpecSheetDbContext dbContext, User user)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        _dbContext = dbContext;
        _user = user;
    }

    /// <summary>
    /// Throws unless the current user may change the draft: it is theirs and isn't awaiting approval.
    /// </summary>
    public async Task EnsureEditableAsync(IDbSheetRevision draft, string item, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(draft, nameof(draft));

        if (draft.SheetSubmissionId is not null)
        {
            var submitter = await NameOfAsync(draft.CreatedByUserId, ct);
            throw new SheetItemLockedException($"This {item} has changes by {submitter} awaiting approval, so it can't be changed until they are approved or rejected.");
        }

        if (draft.CreatedByUserId != _user.Id)
        {
            var holder = await NameOfAsync(draft.CreatedByUserId, ct);
            throw new SheetItemLockedException($"This {item} is checked out to {holder}.");
        }
    }

    /// <summary>
    /// Throws if a row can't be added to the group: someone else is adding or removing the group, or its removal is
    /// awaiting approval.
    /// </summary>
    public async Task EnsureGroupOpenAsync(int groupId, CancellationToken ct)
    {
        var revisions = _dbContext.SheetTableGroupRevisions.Local
            .Where(r => r.SheetTableGroupId == groupId && (r.Status == RevisionStatus.Draft || r.SupersededDateTime == null))
            .Cast<IDbSheetRevision>()
            .ToList();
        if (revisions.Count == 0)
        {
            revisions = [.. await _dbContext.SheetTableGroupRevisions
                .AsNoTracking()
                .Where(r => r.SheetTableGroupId == groupId && (r.Status == RevisionStatus.Draft || r.SupersededDateTime == null))
                .ToListAsync(ct)];
        }

        await EnsureOpenAsync(revisions, "group", ct);
    }

    /// <summary>
    /// Throws if a group can't be added to the table: someone else is adding or removing the table, or its removal is
    /// awaiting approval.
    /// </summary>
    public async Task EnsureTableOpenAsync(int tableId, CancellationToken ct)
    {
        var revisions = _dbContext.SheetTableRevisions.Local
            .Where(r => r.SheetTableId == tableId && (r.Status == RevisionStatus.Draft || r.SupersededDateTime == null))
            .Cast<IDbSheetRevision>()
            .ToList();
        if (revisions.Count == 0)
        {
            revisions = [.. await _dbContext.SheetTableRevisions
                .AsNoTracking()
                .Where(r => r.SheetTableId == tableId && (r.Status == RevisionStatus.Draft || r.SupersededDateTime == null))
                .ToListAsync(ct)];
        }

        await EnsureOpenAsync(revisions, "table", ct);
    }

    /// <summary>
    /// Throws if anyone else has changes to the groups or rows of the table, or any are awaiting approval, so that
    /// removing it doesn't take their work with it.
    /// </summary>
    public async Task EnsureNothingHeldInTableAsync(int tableId, CancellationToken ct)
    {
        var groupIds = _dbContext.SheetTableGroups
            .Where(g => g.SheetTableId == tableId)
            .Select(g => g.Id);

        var holderIds = await _dbContext.SheetTableGroupRevisions
            .Where(r => groupIds.Contains(r.SheetTableGroupId) && r.Status == RevisionStatus.Draft
                && (r.CreatedByUserId != _user.Id || r.SheetSubmissionId != null))
            .Select(r => r.CreatedByUserId)
            .Union(_dbContext.SheetTableRowRevisions
                .Where(r => r.Status == RevisionStatus.Draft
                    && (r.CreatedByUserId != _user.Id || r.SheetSubmissionId != null)
                    && _dbContext.SheetTableRows.Any(row => row.Id == r.SheetTableRowId && groupIds.Contains(row.SheetTableGroupId)))
                .Select(r => r.CreatedByUserId))
            .Distinct()
            .ToListAsync(ct);

        await ThrowIfHeldAsync(holderIds, "table", ct);
    }

    /// <summary>
    /// Throws if anyone else has changes to the rows of the group, or any are awaiting approval.
    /// </summary>
    public async Task EnsureNothingHeldInGroupAsync(int groupId, CancellationToken ct)
    {
        var holderIds = await _dbContext.SheetTableRowRevisions
            .Where(r => r.Status == RevisionStatus.Draft
                && (r.CreatedByUserId != _user.Id || r.SheetSubmissionId != null)
                && _dbContext.SheetTableRows.Any(row => row.Id == r.SheetTableRowId && row.SheetTableGroupId == groupId))
            .Select(r => r.CreatedByUserId)
            .Distinct()
            .ToListAsync(ct);

        await ThrowIfHeldAsync(holderIds, "group", ct);
    }

    private async Task EnsureOpenAsync(IReadOnlyCollection<IDbSheetRevision> revisions, string item, CancellationToken ct)
    {
        var draft = revisions.FirstOrDefault(r => r.Status == RevisionStatus.Draft);
        if (draft is null)
        {
            return;
        }

        var isNew = revisions.All(r => r.Status == RevisionStatus.Draft);
        if (!isNew && !draft.IsDeleted)
        {
            return;
        }

        if (draft.CreatedByUserId == _user.Id && (draft.SheetSubmissionId is null || !draft.IsDeleted))
        {
            return;
        }

        var holder = await NameOfAsync(draft.CreatedByUserId, ct);
        var change = draft.IsDeleted ? "removing" : "adding";
        throw new SheetItemLockedException($"{holder} is {change} this {item}, so nothing can be added to it until that is submitted and approved or undone.");
    }

    private async Task ThrowIfHeldAsync(IReadOnlyCollection<int> holderIds, string item, CancellationToken ct)
    {
        if (holderIds.Count == 0)
        {
            return;
        }

        var names = await _dbContext.Users
            .AsNoTracking()
            .Where(u => holderIds.Contains(u.Id))
            .OrderBy(u => u.DisplayName)
            .Select(u => u.DisplayName)
            .ToListAsync(ct);

        throw new SheetItemLockedException($"This {item} can't be removed while it has changes checked out or awaiting approval ({string.Join(", ", names)}).");
    }

    private async Task<string> NameOfAsync(int userId, CancellationToken ct)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync(ct)
            ?? "another user";
    }
}
