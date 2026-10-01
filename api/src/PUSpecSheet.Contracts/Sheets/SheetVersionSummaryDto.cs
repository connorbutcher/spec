namespace PUSpecSheet.Contracts.Sheets;

/// <summary>One publish of a sheet. Viewing it shows the sheet as it stood when it was published.</summary>
public sealed record SheetVersionSummaryDto(
    int VersionNumber,
    DateTime PublishedAtUtc,
    int PublishedByUserId,
    string PublishedByName,
    string? Note);
