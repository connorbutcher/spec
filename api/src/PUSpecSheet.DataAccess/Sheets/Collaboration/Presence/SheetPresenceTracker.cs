using PUSpecSheet.Abstractions.Sheets.Collaboration;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;

/// <summary>
/// Who has which sheet open, by live connection. Held in memory for the life of the process: presence is
/// only ever about connections to this server, and every client joins again when it reconnects. Row
/// checkouts are not kept here; they are the draft revisions in the database.
/// </summary>
public sealed class SheetPresenceTracker
{
    private readonly Lock _gate = new();

    /// <summary>Every connection, to find what one had open when it leaves.</summary>
    private readonly Dictionary<string, SheetConnection> _connections = new(StringComparer.Ordinal);

    /// <summary>The same connections by sheet, so asking about one sheet doesn't read the others.</summary>
    private readonly Dictionary<int, List<SheetConnection>> _bySheet = [];

    /// <summary>Records that a connection has a sheet open. A connection has one sheet open at a time.</summary>
    public void Join(string connectionId, SheetConnection connection)
    {
        lock (_gate)
        {
            Remove(connectionId);
            _connections[connectionId] = connection;
            if (!_bySheet.TryGetValue(connection.SheetId, out var onSheet))
            {
                onSheet = [];
                _bySheet[connection.SheetId] = onSheet;
            }

            onSheet.Add(connection);
        }
    }

    /// <summary>Forgets a connection. Returns what it had open, or null when it had nothing.</summary>
    public SheetConnection? Leave(string connectionId)
    {
        lock (_gate)
        {
            return Remove(connectionId);
        }
    }

    /// <summary>What a connection has open, or null when it has nothing open.</summary>
    public SheetConnection? Find(string connectionId)
    {
        lock (_gate)
        {
            return _connections.GetValueOrDefault(connectionId);
        }
    }

    /// <summary>One of a person's connections to a sheet, or null when they don't have it open.</summary>
    public (string ConnectionId, SheetConnection Connection)? FindConnectionOf(int sheetId, int userId)
    {
        lock (_gate)
        {
            foreach (var (connectionId, connection) in _connections)
            {
                if (connection.SheetId == sheetId && connection.UserId == userId)
                {
                    return (connectionId, connection);
                }
            }

            return null;
        }
    }

    /// <summary>The people who have the sheet open, by name.</summary>
    public IReadOnlyList<SheetPresenceUser> UsersOn(int sheetId)
    {
        lock (_gate)
        {
            if (!_bySheet.TryGetValue(sheetId, out var onSheet))
            {
                return [];
            }

            return onSheet
                .GroupBy(connection => connection.UserId)
                .Select(user => new SheetPresenceUser
                {
                    UserId = user.Key,
                    DisplayName = user.First().DisplayName,
                    ConnectionCount = user.Count(),
                })
                .OrderBy(user => user.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(user => user.UserId)
                .ToList();
        }
    }

    public bool IsPresent(int sheetId, int userId)
    {
        lock (_gate)
        {
            return _bySheet.TryGetValue(sheetId, out var onSheet)
                && onSheet.Exists(connection => connection.UserId == userId);
        }
    }

    /// <summary>Takes a connection out of both lookups. Call with the gate held.</summary>
    private SheetConnection? Remove(string connectionId)
    {
        if (!_connections.Remove(connectionId, out var connection))
        {
            return null;
        }

        var onSheet = _bySheet[connection.SheetId];

        // Two tabs of one person on one sheet are equal records; either one is the right one to drop.
        onSheet.Remove(connection);
        if (onSheet.Count == 0)
        {
            _bySheet.Remove(connection.SheetId);
        }

        return connection;
    }
}
