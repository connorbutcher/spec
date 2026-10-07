namespace PUSpecSheet.Contracts.Sheets;

/// <summary>How many unpublished changes one person has on a sheet.</summary>
public sealed record SheetDraftSummaryDto(int UserId, string UserName, int DraftCount);
