using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// The rows people have clicked into but not changed yet, each held by the live connection (browser tab)
/// that is in it. Clicking into a cell must not save anything, so these are kept in memory rather than
/// as draft revisions: a tab holds at most one row, and the row is free again the moment the tab moves
/// on, closes or loses its connection. A row someone has actually changed is a draft in the database
/// and is not tracked here.
/// </summary>
public sealed class LiveRowCheckoutTracker
{
    private readonly Lock gate = new();
    private readonly Dictionary<string, LiveRowCheckout> byConnection = new(StringComparer.Ordinal);
    private readonly Dictionary<int, string> connectionByRow = [];

    /// <summary>
    /// Gives the row to a connection, in place of whichever row it held. False, with who holds it, when
    /// someone else is in the row. The same person in another tab simply moves the row to this one.
    /// </summary>
    public bool TryCheckOut(string connectionId, LiveRowCheckout wanted, out LiveRowCheckout? heldBy)
    {
        lock (gate)
        {
            heldBy = Holder(wanted.RowId);
            if (heldBy is not null && heldBy.UserId != wanted.UserId)
            {
                return false;
            }

            ReleaseRowHeld(wanted.RowId);
            ReleaseConnection(connectionId);
            byConnection[connectionId] = wanted;
            connectionByRow[wanted.RowId] = connectionId;
            heldBy = wanted;
            return true;
        }
    }

    /// <summary>Frees whatever row a connection held. Returns it, or null when it held none.</summary>
    public LiveRowCheckout? Release(string connectionId)
    {
        lock (gate)
        {
            return ReleaseConnection(connectionId);
        }
    }

    /// <summary>Frees a row whoever is in it. Returns the checkout that ended, or null when nobody was.</summary>
    public LiveRowCheckout? ReleaseRow(int rowId)
    {
        lock (gate)
        {
            return ReleaseRowHeld(rowId);
        }
    }

    /// <summary>Who is in the row, or null when nobody is.</summary>
    public LiveRowCheckout? HolderOf(int rowId)
    {
        lock (gate)
        {
            return Holder(rowId);
        }
    }

    /// <summary>Every row on a sheet that someone is in, by row id.</summary>
    public IReadOnlyList<RowCheckoutDto> OnSheet(int sheetId)
    {
        lock (gate)
        {
            return byConnection.Values
                .Where(checkout => checkout.SheetId == sheetId)
                .OrderBy(checkout => checkout.RowId)
                .Select(checkout => new RowCheckoutDto(checkout.RowId, checkout.UserId, checkout.DisplayName))
                .ToList();
        }
    }

    private LiveRowCheckout? Holder(int rowId)
    {
        return connectionByRow.TryGetValue(rowId, out var connectionId) ? byConnection[connectionId] : null;
    }

    private LiveRowCheckout? ReleaseConnection(string connectionId)
    {
        if (!byConnection.Remove(connectionId, out var checkout))
        {
            return null;
        }

        connectionByRow.Remove(checkout.RowId);
        return checkout;
    }

    private LiveRowCheckout? ReleaseRowHeld(int rowId)
    {
        return connectionByRow.TryGetValue(rowId, out var connectionId) ? ReleaseConnection(connectionId) : null;
    }
}
