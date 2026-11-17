using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Drafts;

internal static class DraftSummaries
{
    public static int CountOf(IEnumerable<IDbSheetRevision> drafts, int userId)
    {
        return drafts.Count(draft => draft.AuthorUserId == userId);
    }

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
