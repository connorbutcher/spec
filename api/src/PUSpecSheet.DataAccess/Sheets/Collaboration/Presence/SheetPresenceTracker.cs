using PUSpecSheet.Abstractions.Sheets.Collaboration;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;

public sealed class SheetPresenceTracker
{
    private readonly Lock _gate = new();

    private readonly Dictionary<string, SheetConnection> _connections = new(StringComparer.Ordinal);

    // The same connections by sheet, so asking about one sheet doesn't read the others.
    private readonly Dictionary<int, List<SheetConnection>> _bySheet = [];

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

    public SheetConnection? Leave(string connectionId)
    {
        lock (_gate)
        {
            return Remove(connectionId);
        }
    }

    public SheetConnection? Find(string connectionId)
    {
        lock (_gate)
        {
            return _connections.GetValueOrDefault(connectionId);
        }
    }

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
