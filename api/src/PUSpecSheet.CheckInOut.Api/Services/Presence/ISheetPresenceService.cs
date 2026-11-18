using PUSpecSheet.CheckInOut.Api.Models;

namespace PUSpecSheet.CheckInOut.Api.Services.Presence;

/// <summary>
/// Defines a service keeping track of which connections have which sheets open.
/// </summary>
public interface ISheetPresenceService
{
    /// <summary>
    /// Records that the connection has the sheet open.
    /// </summary>
    Task JoinAsync(int sheetId, ConnectedUser user);

    /// <summary>
    /// Records that the connection no longer has the sheet open.
    /// </summary>
    Task LeaveAsync(int sheetId, string connectionId);

    /// <summary>
    /// Forgets every sheet the connection had open, returning their ids.
    /// </summary>
    Task<IReadOnlyList<int>> LeaveAllAsync(string connectionId);

    /// <summary>
    /// Who has the sheet open.
    /// </summary>
    Task<SheetPresence> GetPresenceAsync(int sheetId);
}
