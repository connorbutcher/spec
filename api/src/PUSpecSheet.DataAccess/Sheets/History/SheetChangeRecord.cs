namespace PUSpecSheet.DataAccess.Sheets.History;

public sealed record SheetChangeRecord
{
    public required int VersionNumber { get; init; }

    public required DateTime AtUtc { get; init; }

    public required int AuthorUserId { get; init; }
}
