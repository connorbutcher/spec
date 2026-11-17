namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;

public sealed record SheetConnection
{
    public required int SheetId { get; init; }

    public required int UserId { get; init; }

    public required string DisplayName { get; init; }
}
