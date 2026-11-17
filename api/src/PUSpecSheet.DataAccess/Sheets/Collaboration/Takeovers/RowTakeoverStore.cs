using System.Collections.Concurrent;
using PUSpecSheet.Abstractions.Sheets.Collaboration;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;

// The takeover requests waiting for an answer, one per row at most. Held in memory: a request lives for about a
// minute and only matters to people connected to this server, so one lost to a restart is simply asked again. The
// checkout it is about stays in the database either way.
public sealed class RowTakeoverStore
{
    private readonly ConcurrentDictionary<int, RowTakeover> _pendingByRow = new();

    public bool TryAdd(RowTakeover takeover)
    {
        return _pendingByRow.TryAdd(takeover.RowId, takeover);
    }

    // Takes a request out so it can be settled. False when it has already been taken, which is how two answers
    // arriving together (an approval and the timeout, say) are kept to one outcome.
    public bool TryRemove(RowTakeover takeover)
    {
        return _pendingByRow.TryRemove(new KeyValuePair<int, RowTakeover>(takeover.RowId, takeover));
    }

    public RowTakeover? ForRow(int rowId)
    {
        return _pendingByRow.GetValueOrDefault(rowId);
    }

    public RowTakeover? Find(Guid id)
    {
        return _pendingByRow.Values.FirstOrDefault(takeover => takeover.Id == id);
    }

    public IReadOnlyList<RowTakeover> OnSheet(int sheetId)
    {
        return _pendingByRow.Values.Where(takeover => takeover.SheetId == sheetId).ToList();
    }

    public IReadOnlyList<RowTakeover> Involving(int sheetId, int userId)
    {
        return _pendingByRow.Values
            .Where(takeover => takeover.SheetId == sheetId
                && (takeover.RequesterUserId == userId || takeover.HolderUserId == userId))
            .OrderBy(takeover => takeover.RequestedAtUtc)
            .ToList();
    }

    public IReadOnlyList<RowTakeover> Due(DateTime nowUtc)
    {
        return _pendingByRow.Values.Where(takeover => takeover.ExpiresAtUtc <= nowUtc).ToList();
    }
}
