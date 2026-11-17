using PUSpecSheet.Abstractions.Sheets.Collaboration;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;

public interface IRowTakeoversService
{
    Task<RowTakeoverAvailability> GetAvailabilityAsync(int rowId, CancellationToken cancellationToken);

    // The current user asks for a row. Granted straight away when the holder doesn't have the sheet open; otherwise
    // the holder is asked and the request comes back pending.
    Task<RowTakeover> RequestAsync(int rowId, CancellationToken cancellationToken);

    Task<RowTakeover> ApproveAsync(Guid takeoverId, CancellationToken cancellationToken);

    Task<RowTakeover> DenyAsync(Guid takeoverId, CancellationToken cancellationToken);

    Task<RowTakeover> CancelAsync(Guid takeoverId, CancellationToken cancellationToken);
}
