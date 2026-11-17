using PUSpecSheet.Abstractions.Sheets.Collaboration;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

// The rows people have clicked into but not changed yet, each held by the live connection (browser tab) that is in
// it. Clicking into a cell must not save anything, so these are kept in memory rather than as draft revisions: a tab
// holds at most one row, and the row is free again the moment the tab moves on, closes or loses its connection. A row
// someone has actually changed is a draft in the database and is not tracked here.
public sealed class LiveRowCheckoutTracker
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, LiveRowCheckout> _byConnection = new(StringComparer.Ordinal);
    private readonly Dictionary<int, string> _connectionByRow = [];

    public bool TryCheckOut(string connectionId, LiveRowCheckout wanted, out LiveRowCheckout? heldBy)
    {
        lock (_gate)
        {
            heldBy = Holder(wanted.RowId);
            if (heldBy is not null && heldBy.UserId != wanted.UserId)
            {
                return false;
            }

            ReleaseRowHeld(wanted.RowId);
            ReleaseConnection(connectionId);
            _byConnection[connectionId] = wanted;
            _connectionByRow[wanted.RowId] = connectionId;
            heldBy = wanted;
            return true;
        }
    }

    public LiveRowCheckout? Release(string connectionId)
    {
        lock (_gate)
        {
            return ReleaseConnection(connectionId);
        }
    }

    public LiveRowCheckout? ReleaseRow(int rowId)
    {
        lock (_gate)
        {
            return ReleaseRowHeld(rowId);
        }
    }

    public LiveRowCheckout? HolderOf(int rowId)
    {
        lock (_gate)
        {
            return Holder(rowId);
        }
    }

    public IReadOnlyList<RowCheckout> OnSheet(int sheetId)
    {
        lock (_gate)
        {
            return _byConnection.Values
                .Where(checkout => checkout.SheetId == sheetId)
                .OrderBy(checkout => checkout.RowId)
                .Select(checkout => new RowCheckout
                {
                    RowId = checkout.RowId,
                    UserId = checkout.UserId,
                    UserName = checkout.DisplayName,
                })
                .ToList();
        }
    }

    private LiveRowCheckout? Holder(int rowId)
    {
        return _connectionByRow.TryGetValue(rowId, out var connectionId) ? _byConnection[connectionId] : null;
    }

    private LiveRowCheckout? ReleaseConnection(string connectionId)
    {
        if (!_byConnection.Remove(connectionId, out var checkout))
        {
            return null;
        }

        _connectionByRow.Remove(checkout.RowId);
        return checkout;
    }

    private LiveRowCheckout? ReleaseRowHeld(int rowId)
    {
        return _connectionByRow.TryGetValue(rowId, out var connectionId) ? ReleaseConnection(connectionId) : null;
    }
}
