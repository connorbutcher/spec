using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// Asking for, answering and settling requests to take over a row that is checked out to someone else.
/// A takeover moves the row's draft to the requester as it stands, so the holder's unpublished changes go
/// with it. The draft in the database stays the only record of who a row is checked out to.
/// </summary>
public interface IRowTakeoverService
{
    /// <summary>
    /// The current user asks for a row. Granted straight away when the holder doesn't have the sheet open;
    /// otherwise the holder is asked and the request comes back pending.
    /// </summary>
    /// <exception cref="NotFoundException">The row doesn't exist.</exception>
    /// <exception cref="InvalidRequestException">The row is already checked out to the current user.</exception>
    /// <exception cref="ConflictException">The row isn't checked out, or someone else has already asked for it.</exception>
    Task<RowTakeoverDto> RequestAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>The holder hands the row over.</summary>
    Task<RowTakeoverDto> ApproveAsync(Guid takeoverId, CancellationToken cancellationToken);

    /// <summary>The holder keeps the row.</summary>
    Task<RowTakeoverDto> DenyAsync(Guid takeoverId, CancellationToken cancellationToken);

    /// <summary>The requester withdraws the request.</summary>
    Task<RowTakeoverDto> CancelAsync(Guid takeoverId, CancellationToken cancellationToken);

    /// <summary>Grants every request whose holder has not answered in time.</summary>
    Task GrantOverdueAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Closes the requests on a sheet whose row is no longer checked out to the holder, because they
    /// published or discarded it while the request was waiting.
    /// </summary>
    Task ReleaseSettledAsync(int sheetId, CancellationToken cancellationToken);
}
