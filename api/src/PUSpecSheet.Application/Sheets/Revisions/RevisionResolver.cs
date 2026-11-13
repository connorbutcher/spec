using PUSpecSheet.Database;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Revisions;

public static class RevisionResolver
{
    /// <summary>
    /// Works out what each item shows for <paramref name="currentUserId"/>, from its current published
    /// revision and any drafts. For a past view only published revisions are passed in, so nothing is locked.
    /// </summary>
    public static Dictionary<int, RevisionResolution<TRevision>> Resolve<TRevision>(
        IEnumerable<TRevision> revisions,
        Func<TRevision, int> itemId,
        int currentUserId)
        where TRevision : class, IDbSheetRevision
    {
        return Resolve(revisions, itemId, draft => draft.AuthorUserId == currentUserId);
    }

    /// <summary>
    /// Works out what each item shows, from its current published revision and any draft: the draft when
    /// <paramref name="showsDraft"/> says so, otherwise the published revision.
    /// </summary>
    public static Dictionary<int, RevisionResolution<TRevision>> Resolve<TRevision>(
        IEnumerable<TRevision> revisions,
        Func<TRevision, int> itemId,
        Func<TRevision, bool> showsDraft)
        where TRevision : class, IDbSheetRevision
    {
        var result = new Dictionary<int, RevisionResolution<TRevision>>();
        foreach (var group in revisions.GroupBy(itemId))
        {
            var draft = group.FirstOrDefault(revision => revision.Status == RevisionStatus.Draft);
            var published = group.FirstOrDefault(revision => revision.Status == RevisionStatus.Published);
            var shown = draft is not null && showsDraft(draft) ? draft : published;
            result[group.Key] = new RevisionResolution<TRevision>(shown, draft);
        }

        return result;
    }

    /// <summary>Whether the resolution has something to show that isn't a removal.</summary>
    public static bool IsVisible<TRevision>(RevisionResolution<TRevision>? resolution)
        where TRevision : class, IDbSheetRevision
    {
        return resolution?.Shown is { IsDeleted: false };
    }
}
