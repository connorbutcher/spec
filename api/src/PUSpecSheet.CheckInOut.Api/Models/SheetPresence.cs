namespace PUSpecSheet.CheckInOut.Api.Models;

/// <summary>
/// Who has a sheet open.
/// </summary>
public record SheetPresence
{
    public required int SheetId { get; init; }

    public required IReadOnlyList<ConnectedUser> Users { get; init; }
}
