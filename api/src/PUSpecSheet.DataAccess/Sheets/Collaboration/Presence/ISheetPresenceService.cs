using PUSpecSheet.Abstractions.Sheets.Collaboration;
using PUSpecSheet.DataAccess.Common;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;

/// <summary>Keeps track of who has each sheet open, and tells the others when someone arrives or leaves.</summary>
public interface ISheetPresenceService
{
    /// <summary>Records that a connection opened a sheet as a user. Returns what they need to catch up.</summary>
    /// <exception cref="NotFoundException">The sheet or the user doesn't exist.</exception>
    Task<SheetLiveState> JoinAsync(string connectionId, int sheetId, int userId, CancellationToken cancellationToken);

    /// <summary>Records that a connection closed its sheet or dropped. Returns what it had open, if anything.</summary>
    Task<SheetConnection?> LeaveAsync(string connectionId, CancellationToken cancellationToken);
}
