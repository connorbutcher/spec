namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>How many unpublished changes one person has on a sheet.</summary>
public sealed record SheetDraftSummaryDto
{
    public SheetDraftSummaryDto(int userId, string userName, int draftCount)
    {
        UserId = userId;
        UserName = userName;
        DraftCount = draftCount;
    }

    public int UserId { get; init; }

    public string UserName { get; init; }

    public int DraftCount { get; init; }
}
