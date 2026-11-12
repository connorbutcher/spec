using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Database.Entities.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Queries;

/// <summary>
/// Reads and changes that work the same for every kind of sheet item revision (tables, sections, rows and
/// column blocks), on any of the four revision sets: <c>db.SheetRowRevisions.ListInViewAsync(...)</c>.
/// </summary>
public static class SheetRevisionQueries
{
    /// <summary>
    /// The revisions on the sheet a view can show, read only: for the live view (no moment) each item's
    /// current published revision and any drafts, otherwise the published revision in force at the moment.
    /// </summary>
    public static async Task<List<TRevision>> ListInViewAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int sheetId,
        DateTime? moment,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        return await revisions.OnSheet(sheetId).InView(moment).AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>Every revision on the sheet published up to the moment (or ever, for none), read only.</summary>
    public static async Task<List<TRevision>> ListPublishedUpToAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int sheetId,
        DateTime? moment,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        return await revisions.OnSheet(sheetId).PublishedUpTo(moment).AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>The revisions on the sheet the selection picks, tracked for editing.</summary>
    public static async Task<List<TRevision>> ListSelectedForEditAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int sheetId,
        IDraftSelection selection,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        return await selection.Apply(revisions.OnSheet(sheetId)).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// The items on the sheet with a draft a publish in the scope would take that has an earlier revision to
    /// return to, by the item id <paramref name="itemId"/> picks out.
    /// </summary>
    public static async Task<List<int>> ListItemIdsWithRevisableDraftsAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int sheetId,
        PublishScope scope,
        int userId,
        Expression<Func<TRevision, int>> itemId,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        // Only an item with an earlier revision has something to return to.
        return await revisions.OnSheet(sheetId)
            .DraftsIn(scope, userId)
            .Where(revision => revision.RevisionNumber > 1)
            .Select(itemId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The current published revision and any draft of each of the given revisions' items, tracked.</summary>
    public static async Task<List<TRevision>> ListCurrentAndDraftsForEditAsync<TRevision>(
        this IQueryable<TRevision> itemRevisions,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        return await itemRevisions.CurrentAndDrafts().ToListAsync(cancellationToken);
    }

    /// <summary>The highest revision number among the revisions, or 0 when there are none.</summary>
    public static async Task<int> GetLastRevisionNumberAsync<TRevision>(
        this IQueryable<TRevision> itemRevisions,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        return await itemRevisions.MaxAsync(revision => (int?)revision.RevisionNumber, cancellationToken) ?? 0;
    }

    /// <summary>Closes the current published revision among the revisions at <paramref name="supersededAtUtc"/>, straight in the database.</summary>
    public static async Task SupersedeCurrentAsync<TRevision>(
        this IQueryable<TRevision> itemRevisions,
        DateTime supersededAtUtc,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        await itemRevisions.Current().ExecuteUpdateAsync(
            setters => setters.SetProperty(revision => revision.SupersededAtUtc, supersededAtUtc),
            cancellationToken);
    }

    /// <summary>Hands the drafts of a submission on the sheet back to their authors, straight in the database.</summary>
    public static async Task ReleaseSubmissionAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int sheetId,
        int submissionId,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        await revisions.OnSheet(sheetId).InSubmission(submissionId).ExecuteUpdateAsync(
            setters => setters.SetProperty(revision => revision.SheetSubmissionId, (int?)null),
            cancellationToken);
    }

    /// <summary>Deletes the user's drafts on the sheet that aren't waiting for approval, straight in the database.</summary>
    public static async Task DeleteOpenDraftsAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int sheetId,
        int userId,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        await revisions.OnSheet(sheetId).OpenDraftsOf(userId).ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>How many revisions each of the submissions holds, by submission id.</summary>
    public static async Task<Dictionary<int, int>> CountBySubmissionAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        IReadOnlyCollection<int> submissionIds,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        return await revisions
            .Select(revision => revision.SheetSubmissionId)
            .Where(id => id != null && submissionIds.Contains(id.Value))
            .GroupBy(id => id!.Value)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.Key, entry => entry.Count, cancellationToken);
    }

    /// <summary>
    /// The drafts among the revisions that stop the user removing what contains them: other people's, and
    /// the user's own once they are waiting for approval.
    /// </summary>
    public static async Task<List<ContainedHold>> ListHoldsAsync<TRevision>(
        this IQueryable<TRevision> revisions,
        int userId,
        CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        return await revisions
            .Where(revision => revision.Status == RevisionStatus.Draft && (revision.AuthorUserId != userId || revision.SheetSubmissionId != null))
            .Select(revision => new ContainedHold(revision.AuthorUserId, revision.SheetSubmissionId != null))
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<TRevision> OnSheet<TRevision>(this IQueryable<TRevision> revisions, int sheetId)
        where TRevision : class, ISheetRevision
    {
        return revisions.Where(revision => revision.SheetId == sheetId);
    }
}
