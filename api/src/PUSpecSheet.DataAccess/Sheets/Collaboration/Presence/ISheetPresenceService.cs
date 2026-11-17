using PUSpecSheet.Abstractions.Sheets.Collaboration;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;

public interface ISheetPresenceService
{
    Task<SheetLiveState> JoinAsync(string connectionId, int sheetId, int userId, CancellationToken cancellationToken);

    Task<SheetConnection?> LeaveAsync(string connectionId, CancellationToken cancellationToken);
}
