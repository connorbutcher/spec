using PUSpecSheet.Application.Common;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// Checking a row out by clicking into it, before anything in it has changed, and letting it go again.
/// The checkout belongs to the browser tab's live connection and tells everyone on the sheet as it
/// comes and goes.
/// </summary>
public interface ILiveRowCheckoutService
{
    /// <summary>The connection's user enters a row on the sheet the connection has open, leaving any row it was in.</summary>
    /// <exception cref="NotFoundException">The row doesn't exist.</exception>
    /// <exception cref="InvalidRequestException">The connection has no sheet open, or the row is on another sheet.</exception>
    /// <exception cref="ConflictException">Someone else is in the row, or has unpublished changes on it.</exception>
    Task CheckOutAsync(string connectionId, int rowId, CancellationToken cancellationToken);

    /// <summary>The connection leaves the row it was in, if it was in one.</summary>
    Task ReleaseAsync(string connectionId, CancellationToken cancellationToken);
}
