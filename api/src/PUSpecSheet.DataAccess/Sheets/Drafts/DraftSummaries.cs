using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Drafts;

/// <summary>Counts a sheet's drafts by who holds them, which is what the publish choice shows.</summary>
internal static class DraftSummaries
{
    public static int CountOf(IEnumerable<IDbSheetRevision> drafts, int userId)
    {
        return drafts.Count(draft => draft.AuthorUserId == userId);
    }

    /// <summary>Everyone but <paramref name="userId"/> who holds a draft, by name, with how many they hold.</summary>
    public static List<SheetDraftSummary> OfOthers(
        IEnumerable<IDbSheetRevision> drafts,
        int userId,
        Func<int, string> userName)
    {
        return drafts
            .Where(draft => draft.AuthorUserId != userId)
            .GroupBy(draft => draft.AuthorUserId)
            .Select(group => new SheetDraftSummary { UserId = group.Key, UserName = userName(group.Key), DraftCount = group.Count() })
            .OrderBy(summary => summary.UserName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(summary => summary.UserId)
            .ToList();
    }
}
