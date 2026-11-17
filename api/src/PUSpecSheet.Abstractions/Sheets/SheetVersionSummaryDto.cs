namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>One publish of a sheet. Viewing it shows the sheet as it stood when it was published.</summary>
public sealed record SheetVersionSummaryDto
{
    public required int VersionNumber { get; init; }

    public required DateTime PublishedAtUtc { get; init; }

    public required int PublishedByUserId { get; init; }

    public required string PublishedByName { get; init; }

    public string? Note { get; init; }
}
