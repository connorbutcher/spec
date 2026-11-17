namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

// Who rows are checked out to. A row is checked out either because someone has changed it (its draft revision in the
// database) or because someone is in it without having changed it yet (held in memory by LiveRowCheckoutTracker).
// This puts the two behind one question, and is the seam the takeover and checkout rules' tests replace.
public interface IRowCheckouts
{
    Task<RowCheckoutState?> FindAsync(int rowId, CancellationToken cancellationToken);

    Task<int?> DraftHolderAsync(int rowId, CancellationToken cancellationToken);

    Task<int?> SheetOfAsync(int rowId, CancellationToken cancellationToken);

    Task<RowTransfer> TransferAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, string>> DisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken);
}
