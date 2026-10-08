using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// What people do with takeover requests: ask for a row that is checked out to someone else, answer a
/// request for one of their own rows, or withdraw a request they made. Only a row its holder has not
/// changed can be taken over: once changed, it stays with them until they publish or discard.
/// </summary>
public interface IRowTakeoverService
{
    /// <summary>Whether the current user could ask for a row right now, and the reason when they couldn't.</summary>
    /// <exception cref="NotFoundException">The row doesn't exist.</exception>
    Task<RowTakeoverAvailabilityDto> GetAvailabilityAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>
    /// The current user asks for a row. Granted straight away when the holder doesn't have the sheet open;
    /// otherwise the holder is asked and the request comes back pending.
    /// </summary>
    /// <exception cref="NotFoundException">The row doesn't exist.</exception>
    /// <exception cref="InvalidRequestException">The row is already checked out to the current user.</exception>
    /// <exception cref="ConflictException">
    /// The row isn't checked out, has unpublished changes, or someone else has already asked for it.
    /// </exception>
    Task<RowTakeoverDto> RequestAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>The holder hands the row over.</summary>
    /// <exception cref="NotFoundException">The request is no longer open.</exception>
    /// <exception cref="InvalidRequestException">The current user is not the holder.</exception>
    Task<RowTakeoverDto> ApproveAsync(Guid takeoverId, CancellationToken cancellationToken);

    /// <summary>The holder keeps the row.</summary>
    /// <exception cref="NotFoundException">The request is no longer open.</exception>
    /// <exception cref="InvalidRequestException">The current user is not the holder.</exception>
    Task<RowTakeoverDto> DenyAsync(Guid takeoverId, CancellationToken cancellationToken);

    /// <summary>The requester withdraws the request.</summary>
    /// <exception cref="NotFoundException">The request is no longer open.</exception>
    /// <exception cref="InvalidRequestException">The current user is not the requester.</exception>
    Task<RowTakeoverDto> CancelAsync(Guid takeoverId, CancellationToken cancellationToken);
}
