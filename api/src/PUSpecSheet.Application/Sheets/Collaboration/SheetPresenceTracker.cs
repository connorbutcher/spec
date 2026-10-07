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
    private readonly Dictionary<string, SheetConnection> connections = new(StringComparer.Ordinal);

    /// <summary>Records that a connection has a sheet open. A connection has one sheet open at a time.</summary>
    public void Join(string connectionId, SheetConnection connection)
    {
        lock (gate)
        {
            connections[connectionId] = connection;
        }
    }

    /// <summary>Forgets a connection. Returns what it had open, or null when it had nothing.</summary>
    public SheetConnection? Leave(string connectionId)
    {
        lock (gate)
        {
            return connections.Remove(connectionId, out var connection) ? connection : null;
        }
    }

    /// <summary>The people who have the sheet open, by name.</summary>
    public IReadOnlyList<SheetPresenceUserDto> UsersOn(int sheetId)
    {
        lock (gate)
        {
            return connections.Values
                .Where(connection => connection.SheetId == sheetId)
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
            return connections.Values.Any(connection => connection.SheetId == sheetId && connection.UserId == userId);
        }
    }
}
