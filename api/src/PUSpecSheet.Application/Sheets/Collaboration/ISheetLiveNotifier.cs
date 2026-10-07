using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// Tells the people who have a sheet open what has just happened on it. The API implements this over
/// SignalR; the application only says what to tell and to whom.
/// </summary>
public interface ISheetLiveNotifier
{
    /// <summary>Tells everyone on the sheet who has it open now.</summary>
    Task PresenceChangedAsync(int sheetId, IReadOnlyList<SheetPresenceUserDto> users, CancellationToken cancellationToken);

    /// <summary>
    /// Tells everyone on the sheet that what they see is out of date (a row was checked out or released, or
    /// a version was published), leaving out the connection that made the change, which already has it.
    /// </summary>
    Task SheetChangedAsync(int sheetId, string? exceptConnectionId, CancellationToken cancellationToken);

    /// <summary>Tells the requester and the holder of a takeover request where it has got to.</summary>
    Task TakeoverChangedAsync(RowTakeoverDto takeover, CancellationToken cancellationToken);
}
