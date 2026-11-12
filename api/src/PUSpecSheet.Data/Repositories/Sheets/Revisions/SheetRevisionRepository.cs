using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Repositories.Sheets.Revisions;

/// <summary>
/// The queries every kind of revision shares. Revisions are found by the sheet each one carries, which the
/// (sheet, status) and (sheet, published) indexes lead with, so the cost follows the size of the sheet
/// rather than the whole database. Each subclass names its table and the item a revision belongs to,
/// which EF needs written against the concrete entity.
/// </summary>
public abstract class SheetRevisionRepository<TRevision> : ISheetRevisionRepository<TRevision>
    where TRevision : class, ISheetRevision
{
    private readonly PuSpecSheetDbContext db;

    protected SheetRevisionRepository(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    protected PuSpecSheetDbContext Db => db;

    protected abstract DbSet<TRevision> Revisions { get; }

    /// <summary>The id of the item a revision belongs to.</summary>
    protected abstract Expression<Func<TRevision, int>> ItemId { get; }

    public async Task<List<TRevision>> ListCurrentAndDraftsForEditAsync(IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken)
    {
        return await OfItems(itemIds).CurrentAndDrafts().ToListAsync(cancellationToken);
    }

    public async Task<int> GetLastRevisionNumberAsync(int itemId, CancellationToken cancellationToken)
    {
        return await OfItem(itemId).MaxAsync(revision => (int?)revision.RevisionNumber, cancellationToken) ?? 0;
    }

    public async Task<List<TRevision>> ListInViewAsync(int sheetId, DateTime? moment, CancellationToken cancellationToken)
    {
        return await OnSheet(sheetId).InView(moment).AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<TRevision>> ListPublishedUpToAsync(int sheetId, DateTime? moment, CancellationToken cancellationToken)
    {
        return await OnSheet(sheetId).PublishedUpTo(moment).AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<TRevision>> ListSelectedForEditAsync(int sheetId, IDraftSelection selection, CancellationToken cancellationToken)
    {
        return await selection.Apply(OnSheet(sheetId)).ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListItemIdsWithRevisableDraftsAsync(int sheetId, PublishScope scope, int userId, CancellationToken cancellationToken)
    {
        // Only an item with an earlier revision has something to return to.
        return await OnSheet(sheetId)
            .DraftsIn(scope, userId)
            .Where(revision => revision.RevisionNumber > 1)
            .Select(ItemId)
            .ToListAsync(cancellationToken);
    }

    public async Task SupersedeCurrentAsync(IReadOnlyCollection<int> itemIds, DateTime supersededAtUtc, CancellationToken cancellationToken)
    {
        await OfItems(itemIds).Current().ExecuteUpdateAsync(
            setters => setters.SetProperty(revision => revision.SupersededAtUtc, supersededAtUtc),
            cancellationToken);
    }

    public async Task ReleaseSubmissionAsync(int sheetId, int submissionId, CancellationToken cancellationToken)
    {
        await OnSheet(sheetId).InSubmission(submissionId).ExecuteUpdateAsync(
            setters => setters.SetProperty(revision => revision.SheetSubmissionId, (int?)null),
            cancellationToken);
    }

    public async Task DeleteOpenDraftsAsync(int sheetId, int userId, CancellationToken cancellationToken)
    {
        await OnSheet(sheetId).OpenDraftsOf(userId).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<Dictionary<int, int>> CountBySubmissionAsync(IReadOnlyCollection<int> submissionIds, CancellationToken cancellationToken)
    {
        return await Revisions
            .Select(revision => revision.SheetSubmissionId)
            .Where(id => id != null && submissionIds.Contains(id.Value))
            .GroupBy(id => id!.Value)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.Key, entry => entry.Count, cancellationToken);
    }

    public void Add(TRevision revision, int itemId)
    {
        SetItem(revision, itemId);
        Revisions.Add(revision);
    }

    public void Remove(TRevision revision)
    {
        Revisions.Remove(revision);
    }

    public void RemoveRange(IEnumerable<TRevision> revisions)
    {
        Revisions.RemoveRange(revisions);
    }

    /// <summary>The revisions of the items.</summary>
    protected abstract IQueryable<TRevision> OfItems(IReadOnlyCollection<int> itemIds);

    /// <summary>The revisions of one item.</summary>
    protected abstract IQueryable<TRevision> OfItem(int itemId);

    protected abstract void SetItem(TRevision revision, int itemId);

    /// <summary>Every revision of this kind on the sheet, found by the sheet each revision carries.</summary>
    protected IQueryable<TRevision> OnSheet(int sheetId)
    {
        return Revisions.Where(revision => revision.SheetId == sheetId);
    }

    /// <summary>The drafts in a query that someone other than the user holds, or that wait for approval.</summary>
    protected async Task<List<ContainedHold>> ListHoldsAsync(
        IQueryable<TRevision> revisions,
        int userId,
        CancellationToken cancellationToken)
    {
        return await revisions
            .Where(revision => revision.Status == RevisionStatus.Draft && (revision.AuthorUserId != userId || revision.SheetSubmissionId != null))
            .Select(revision => new ContainedHold(revision.AuthorUserId, revision.SheetSubmissionId != null))
            .ToListAsync(cancellationToken);
    }
}
