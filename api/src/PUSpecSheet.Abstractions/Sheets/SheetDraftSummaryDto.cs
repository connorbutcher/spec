namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>How many unpublished changes one person has on a sheet.</summary>
public sealed record SheetDraftSummaryDto
{
    public required int UserId { get; init; }

    public required string UserName { get; init; }

    public required int DraftCount { get; init; }
}
