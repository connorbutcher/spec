using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>
/// The revisions of one kind of sheet item (table, section, row or column block). Every query a draft,
/// publish or view makes of them is written once here; the item each revision belongs to is the only thing
/// that differs between the kinds.
/// </summary>
public interface ISheetRevisionRepository<TRevision>
    where TRevision : class, ISheetRevision
{
    /// <summary>The current published revision and any draft of each of the items, tracked.</summary>
    Task<List<TRevision>> ListCurrentAndDraftsForEditAsync(IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken);

    /// <summary>The highest revision number the item has had, or 0 when it has none.</summary>
    Task<int> GetLastRevisionNumberAsync(int itemId, CancellationToken cancellationToken);

    /// <summary>
    /// The revisions on the sheet a view can show, read only: for the live view (no moment) each item's
    /// current published revision and any drafts, otherwise the published revision in force at the moment.
    /// </summary>
    Task<List<TRevision>> ListInViewAsync(int sheetId, DateTime? moment, CancellationToken cancellationToken);

    /// <summary>Every revision on the sheet published up to the moment (or ever, for none), read only.</summary>
    Task<List<TRevision>> ListPublishedUpToAsync(int sheetId, DateTime? moment, CancellationToken cancellationToken);

    /// <summary>The drafts on the sheet that a publish or submission takes, tracked.</summary>
    Task<List<TRevision>> ListSelectedForEditAsync(int sheetId, IDraftSelection selection, CancellationToken cancellationToken);

    /// <summary>The items on the sheet with a draft in the publish scope that has an earlier revision to return to.</summary>
    Task<List<int>> ListItemIdsWithRevisableDraftsAsync(int sheetId, PublishScope scope, int userId, CancellationToken cancellationToken);

    /// <summary>Closes the current published revision of each item at <paramref name="supersededAtUtc"/>, straight in the database.</summary>
    Task SupersedeCurrentAsync(IReadOnlyCollection<int> itemIds, DateTime supersededAtUtc, CancellationToken cancellationToken);

    /// <summary>Hands the drafts of a submission on the sheet back to their authors, straight in the database.</summary>
    Task ReleaseSubmissionAsync(int sheetId, int submissionId, CancellationToken cancellationToken);

    /// <summary>Deletes the user's drafts on the sheet that aren't waiting for approval, straight in the database.</summary>
    Task DeleteOpenDraftsAsync(int sheetId, int userId, CancellationToken cancellationToken);

    /// <summary>How many revisions each of the submissions holds, by submission id.</summary>
    Task<Dictionary<int, int>> CountBySubmissionAsync(IReadOnlyCollection<int> submissionIds, CancellationToken cancellationToken);

    /// <summary>Points a new revision at its item and adds it.</summary>
    void Add(TRevision revision, int itemId);

    void Remove(TRevision revision);

    void RemoveRange(IEnumerable<TRevision> revisions);
}
