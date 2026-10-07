using System.Collections.Concurrent;
using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// The takeover requests waiting for an answer, one per row at most. Held in memory: a request lives for
/// about a minute and only matters to people connected to this server, so one lost to a restart is simply
/// asked again. The checkout it is about stays in the database either way.
/// </summary>
public sealed class RowTakeoverStore
{
    private readonly ConcurrentDictionary<int, RowTakeoverDto> pendingByRow = new();

    /// <summary>Adds a request. False when the row already has one waiting.</summary>
    public bool TryAdd(RowTakeoverDto takeover)
    {
        return pendingByRow.TryAdd(takeover.RowId, takeover);
    }

    /// <summary>
    /// Takes a request out so it can be settled. False when it has already been taken, which is how two
    /// answers arriving together (an approval and the timeout, say) are kept to one outcome.
    /// </summary>
    public bool TryRemove(RowTakeoverDto takeover)
    {
        return pendingByRow.TryRemove(new KeyValuePair<int, RowTakeoverDto>(takeover.RowId, takeover));
    }

    public RowTakeoverDto? ForRow(int rowId)
    {
        return pendingByRow.GetValueOrDefault(rowId);
    }

    public RowTakeoverDto? Find(Guid id)
    {
        return pendingByRow.Values.FirstOrDefault(takeover => takeover.Id == id);
    }

    public IReadOnlyList<RowTakeoverDto> OnSheet(int sheetId)
    {
        return pendingByRow.Values.Where(takeover => takeover.SheetId == sheetId).ToList();
    }

    /// <summary>The requests on a sheet that a user made or has to answer, oldest first.</summary>
    public IReadOnlyList<RowTakeoverDto> Involving(int sheetId, int userId)
    {
        return pendingByRow.Values
            .Where(takeover => takeover.SheetId == sheetId
                && (takeover.RequesterUserId == userId || takeover.HolderUserId == userId))
            .OrderBy(takeover => takeover.RequestedAtUtc)
            .ToList();
    }

    /// <summary>The requests whose time to answer has run out.</summary>
    public IReadOnlyList<RowTakeoverDto> Due(DateTime nowUtc)
    {
        return pendingByRow.Values.Where(takeover => takeover.ExpiresAtUtc <= nowUtc).ToList();
    }
}
