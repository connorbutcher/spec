using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

internal static class RevisionResolver
{
    /// <summary>
    /// Works out what each item shows for <paramref name="currentUserId"/>, from its current published
    /// revision and any drafts. For a past view only published revisions are passed in, so nothing is locked.
    /// </summary>
    public static Dictionary<int, RevisionResolution<TRevision>> Resolve<TRevision>(
        IEnumerable<TRevision> revisions,
        Func<TRevision, int> itemId,
        int currentUserId)
        where TRevision : class, ISheetRevision
    {
        var result = new Dictionary<int, RevisionResolution<TRevision>>();
        foreach (var group in revisions.GroupBy(itemId))
        {
            var draft = group.FirstOrDefault(revision => revision.Status == RevisionStatus.Draft);
            var published = group.FirstOrDefault(revision => revision.Status == RevisionStatus.Published);
            var shown = draft is not null && draft.AuthorUserId == currentUserId ? draft : published;
            result[group.Key] = new RevisionResolution<TRevision>(shown, draft);
        }

        return result;
    }

    /// <summary>Whether the resolution has something to show that isn't a removal.</summary>
    public static bool IsVisible<TRevision>(RevisionResolution<TRevision>? resolution)
        where TRevision : class, ISheetRevision
    {
        return resolution?.Shown is { IsDeleted: false };
    }
}
