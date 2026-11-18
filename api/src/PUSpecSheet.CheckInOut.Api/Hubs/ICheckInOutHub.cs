using PUSpecSheet.CheckInOut.Api.Models;

namespace PUSpecSheet.CheckInOut.Api.Hubs;

public interface ICheckInOutHub
{
    /// <summary>
    /// Tells everyone viewing a sheet who has it open.
    /// </summary>
    /// <param name="presence">Who has the sheet open.</param>
    Task SheetPresence(SheetPresence presence);

    /// <summary>
    /// Tells the others viewing a sheet that a user saved, undid, submitted or decided changes on it, so they read its
    /// checkouts again.
    /// </summary>
    /// <param name="notice">The sheet, the user and what they did.</param>
    Task SheetChanged(SheetChangedNotice notice);
}
