namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

public interface ILiveRowCheckoutService
{
    Task CheckOutAsync(string connectionId, int rowId, CancellationToken cancellationToken);

    Task ReleaseAsync(string connectionId, CancellationToken cancellationToken);
}
