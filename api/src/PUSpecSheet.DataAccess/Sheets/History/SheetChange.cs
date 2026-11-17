namespace PUSpecSheet.DataAccess.Sheets.History;

/// <summary>The published version in which something on a sheet last changed, when, and who published it.</summary>
public sealed record SheetChange
{
    public required int VersionNumber { get; init; }

    public required DateTime AtUtc { get; init; }

    public required int AuthorUserId { get; init; }
}
