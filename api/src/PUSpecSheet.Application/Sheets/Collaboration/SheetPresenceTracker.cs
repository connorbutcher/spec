using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// Who has which sheet open, by live connection. Held in memory for the life of the process: presence is
/// only ever about connections to this server, and every client joins again when it reconnects. Row
/// checkouts are not kept here; they are the draft revisions in the database.
/// </summary>
public sealed class SheetPresenceTracker
{
    private readonly Lock gate = new();

    /// <summary>Every connection, to find what one had open when it leaves.</summary>
    private readonly Dictionary<string, SheetConnection> connections = new(StringComparer.Ordinal);

    /// <summary>The same connections by sheet, so asking about one sheet doesn't read the others.</summary>
    private readonly Dictionary<int, List<SheetConnection>> bySheet = [];

    /// <summary>Records that a connection has a sheet open. A connection has one sheet open at a time.</summary>
    public void Join(string connectionId, SheetConnection connection)
    {
        lock (gate)
        {
            Remove(connectionId);
            connections[connectionId] = connection;
            if (!bySheet.TryGetValue(connection.SheetId, out var onSheet))
            {
                onSheet = [];
                bySheet[connection.SheetId] = onSheet;
            }

            onSheet.Add(connection);
        }
    }

    /// <summary>Forgets a connection. Returns what it had open, or null when it had nothing.</summary>
    public SheetConnection? Leave(string connectionId)
    {
        lock (gate)
        {
            return Remove(connectionId);
        }
    }

    /// <summary>What a connection has open, or null when it has nothing open.</summary>
    public SheetConnection? Find(string connectionId)
    {
        lock (gate)
        {
            return connections.GetValueOrDefault(connectionId);
        }
    }

    /// <summary>One of a person's connections to a sheet, or null when they don't have it open.</summary>
    public (string ConnectionId, SheetConnection Connection)? FindConnectionOf(int sheetId, int userId)
    {
        lock (gate)
        {
            foreach (var (connectionId, connection) in connections)
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
    public IReadOnlyList<SheetPresenceUserDto> UsersOn(int sheetId)
    {
        lock (gate)
        {
            if (!bySheet.TryGetValue(sheetId, out var onSheet))
            {
                return [];
            }

            return onSheet
                .GroupBy(connection => connection.UserId)
                .Select(user => new SheetPresenceUserDto(user.Key, user.First().DisplayName, user.Count()))
                .OrderBy(user => user.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(user => user.UserId)
                .ToList();
        }
    }

    public bool IsPresent(int sheetId, int userId)
    {
        lock (gate)
        {
            return bySheet.TryGetValue(sheetId, out var onSheet)
                && onSheet.Exists(connection => connection.UserId == userId);
        }
    }

    /// <summary>Takes a connection out of both lookups. Call with the gate held.</summary>
    private SheetConnection? Remove(string connectionId)
    {
        if (!connections.Remove(connectionId, out var connection))
        {
            return null;
        }

        var onSheet = bySheet[connection.SheetId];

        // Two tabs of one person on one sheet are equal records; either one is the right one to drop.
        onSheet.Remove(connection);
        if (onSheet.Count == 0)
        {
            bySheet.Remove(connection.SheetId);
        }

        return connection;
    }
}
