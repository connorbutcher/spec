using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Sheets.Drafts;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Queries;

public static class SheetRevisionQueries
{
    // The revisions on the sheet a view can show, read only: for the live view (no moment) each item's current
    // published revision and any drafts, otherwise the published revision in force at the moment.
    public static async Task<List<TRevision>> ListInViewAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int sheetId,
        DateTime? moment,
        CancellationToken cancellationToken)
        where TRevision : class, IDbSheetRevision
    {
        return await revisions.OnSheet(sheetId).InView(moment).AsNoTracking().ToListAsync(cancellationToken);
    }

    public static async Task<List<TRevision>> ListPublishedUpToAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int sheetId,
        DateTime? moment,
        CancellationToken cancellationToken)
        where TRevision : class, IDbSheetRevision
    {
        return await revisions.OnSheet(sheetId).PublishedUpTo(moment).AsNoTracking().ToListAsync(cancellationToken);
    }

    public static async Task<List<TRevision>> ListSelectedForEditAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int sheetId,
        DraftSelection selection,
        CancellationToken cancellationToken)
        where TRevision : class, IDbSheetRevision
    {
        return await selection.Apply(revisions.OnSheet(sheetId)).ToListAsync(cancellationToken);
    }

    // The items on the sheet with a draft a publish in the scope would take that has an earlier revision to return
    // to, by the item id itemId picks out.
    public static async Task<List<int>> ListItemIdsWithRevisableDraftsAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int sheetId,
        PublishScope scope,
        int userId,
        Expression<Func<TRevision, int>> itemId,
        CancellationToken cancellationToken)
        where TRevision : class, IDbSheetRevision
    {
        // Only an item with an earlier revision has something to return to.
        return await revisions.OnSheet(sheetId)
            .DraftsIn(scope, userId)
            .Where(revision => revision.RevisionNumber > 1)
            .Select(itemId)
            .ToListAsync(cancellationToken);
    }

    public static async Task<List<TRevision>> ListCurrentAndDraftsForEditAsync<TRevision>(
        this IQueryable<TRevision> itemRevisions,
        CancellationToken cancellationToken)
        where TRevision : class, IDbSheetRevision
    {
        return await itemRevisions.CurrentAndDrafts().ToListAsync(cancellationToken);
    }

    public static async Task<int> GetLastRevisionNumberAsync<TRevision>(
        this IQueryable<TRevision> itemRevisions,
        CancellationToken cancellationToken)
        where TRevision : class, IDbSheetRevision
    {
        return await itemRevisions.MaxAsync(revision => (int?)revision.RevisionNumber, cancellationToken) ?? 0;
    }

    public static async Task SupersedeCurrentAsync<TRevision>(
        this IQueryable<TRevision> itemRevisions,
        DateTime supersededAtUtc,
        CancellationToken cancellationToken)
        where TRevision : class, IDbSheetRevision
    {
        await itemRevisions.Current().ExecuteUpdateAsync(
            setters => setters.SetProperty(revision => revision.SupersededAtUtc, supersededAtUtc),
            cancellationToken);
    }

    public static async Task ReleaseSubmissionAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int sheetId,
        int submissionId,
        CancellationToken cancellationToken)
        where TRevision : class, IDbSheetRevision
    {
        await revisions.OnSheet(sheetId).InSubmission(submissionId).ExecuteUpdateAsync(
            setters => setters.SetProperty(revision => revision.SheetSubmissionId, (int?)null),
            cancellationToken);
    }

    public static async Task<Dictionary<int, int>> CountBySubmissionAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        IReadOnlyCollection<int> submissionIds,
        CancellationToken cancellationToken)
        where TRevision : class, IDbSheetRevision
    {
        return await revisions
            .Select(revision => revision.SheetSubmissionId)
            .Where(id => id != null && submissionIds.Contains(id.Value))
            .GroupBy(id => id!.Value)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.Key, entry => entry.Count, cancellationToken);
    }

    public static async Task<List<ContainedHold>> ListHoldsAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int userId,
        CancellationToken cancellationToken)
        where TRevision : class, IDbSheetRevision
    {
        return await revisions
            .Where(revision => revision.Status == RevisionStatus.Draft && (revision.AuthorUserId != userId || revision.SheetSubmissionId != null))
            .Select(revision => new ContainedHold { UserId = revision.AuthorUserId, AwaitingApproval = revision.SheetSubmissionId != null })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<TRevision> OnSheet<TRevision>(this IQueryable<TRevision> revisions, int sheetId)
        where TRevision : class, IDbSheetRevision
    {
        return revisions.Where(revision => revision.SheetId == sheetId);
    }
}
