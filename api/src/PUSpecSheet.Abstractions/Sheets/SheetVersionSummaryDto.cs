namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>One publish of a sheet. Viewing it shows the sheet as it stood when it was published.</summary>
public sealed record SheetVersionSummaryDto
{
    public SheetVersionSummaryDto(int versionNumber, DateTime publishedAtUtc, int publishedByUserId, string publishedByName, string? note)
    {
        VersionNumber = versionNumber;
        PublishedAtUtc = publishedAtUtc;
        PublishedByUserId = publishedByUserId;
        PublishedByName = publishedByName;
        Note = note;
    }

    public int VersionNumber { get; init; }

    public DateTime PublishedAtUtc { get; init; }

    public int PublishedByUserId { get; init; }

    public string PublishedByName { get; init; }

    public string? Note { get; init; }
}
