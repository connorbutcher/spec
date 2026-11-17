namespace PUSpecSheet.DataAccess.Sheets.History;

/// <summary>The published version in which something on a sheet last changed, when, and who published it.</summary>
public sealed record SheetChange
{
    public SheetChange(int versionNumber, DateTime atUtc, int authorUserId)
    {
        VersionNumber = versionNumber;
        AtUtc = atUtc;
        AuthorUserId = authorUserId;
    }

    public int VersionNumber { get; init; }

    public DateTime AtUtc { get; init; }

    public int AuthorUserId { get; init; }
}
