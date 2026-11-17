using System.Globalization;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// Gives each item in a copy a key that follows it from one copy to the next: its ReferenceID, which a copy keeps when
/// it is made from another. An item whose ReferenceID turns up under another parent or template than before is a new
/// item, since an item can't move or change its template; the warning says so.
/// </summary>
internal sealed class KeyAssigner
{
    private readonly Dictionary<(string Kind, Guid ReferenceId), (int Key, string Place)> _known = [];
    private readonly List<string> _warnings;
    private int _nextKey = 1;

    public KeyAssigner(List<string> warnings)
    {
        _warnings = warnings;
    }

    public SnapshotKeys Assign(SheetSnapshot copy)
    {
        var keys = new SnapshotKeys();
        var usedTables = new HashSet<int>();
        foreach (var table in copy.Tables)
        {
            keys.Tables[table.Id] = KeyFor(usedTables, "table", table.ReferenceId, Place(table.TableTemplateId));
        }

        var usedGroups = new HashSet<int>();
        foreach (var group in copy.Groups)
        {
            keys.Groups[group.Id] = KeyFor(usedGroups, "group", group.ReferenceId, Place(keys.Tables[group.TableId], group.TableGroupTemplateId));
        }

        var usedRows = new HashSet<int>();
        foreach (var row in copy.Rows)
        {
            keys.Rows[row.Id] = KeyFor(usedRows, "row", row.ReferenceId, Place(keys.Groups[row.GroupId], row.TableRowTemplateId));
        }

        return keys;
    }

    // usedInCopy holds the keys of the kind already given out in this copy, as a set so each check is one lookup
    // rather than a pass over every key so far. The key given out is added to it.
    private int KeyFor(HashSet<int> usedInCopy, string kind, Guid referenceId, string place)
    {
        if (_known.TryGetValue((kind, referenceId), out var known) && !usedInCopy.Contains(known.Key))
        {
            if (string.Equals(known.Place, place, StringComparison.Ordinal))
            {
                usedInCopy.Add(known.Key);
                return known.Key;
            }

            _warnings.Add($"The {kind} {referenceId} moved to another parent or template, which an item can't do; it is removed from where it was and added again.");
        }
        else if (_known.ContainsKey((kind, referenceId)))
        {
            _warnings.Add($"The ReferenceID {referenceId} is on more than one {kind} in one copy; the second is treated as a new {kind}.");
        }

        var key = _nextKey++;
        _known[(kind, referenceId)] = (key, place);
        usedInCopy.Add(key);
        return key;
    }

    private static string Place(params int[] parts)
    {
        return string.Join('/', parts.Select(part => part.ToString(CultureInfo.InvariantCulture)));
    }
}
