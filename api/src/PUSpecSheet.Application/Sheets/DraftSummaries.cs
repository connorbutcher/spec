using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Counts a sheet's drafts by who holds them, which is what the publish choice shows.</summary>
internal static class DraftSummaries
{
    public static int CountOf(IEnumerable<ISheetRevision> drafts, int userId)
    {
        return drafts.Count(draft => draft.AuthorUserId == userId);
    }

    /// <summary>Everyone but <paramref name="userId"/> who holds a draft, by name, with how many they hold.</summary>
    public static List<SheetDraftSummaryDto> OfOthers(
        IEnumerable<ISheetRevision> drafts,
        int userId,
        Func<int, string> userName)
    {
        return drafts
            .Where(draft => draft.AuthorUserId != userId)
            .GroupBy(draft => draft.AuthorUserId)
            .Select(group => new SheetDraftSummaryDto(group.Key, userName(group.Key), group.Count()))
            .OrderBy(summary => summary.UserName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(summary => summary.UserId)
            .ToList();
    }
}
