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
        foreach (var table in copy.Tables)
        {
            keys.Tables[table.Id] = KeyFor(keys.Tables.Values, "table", table.ReferenceId, Place(table.TableTemplateId));
        }

        foreach (var group in copy.Groups)
        {
            keys.Groups[group.Id] = KeyFor(keys.Groups.Values, "group", group.ReferenceId, Place(keys.Tables[group.TableId], group.TableGroupTemplateId));
        }

        foreach (var row in copy.Rows)
        {
            keys.Rows[row.Id] = KeyFor(keys.Rows.Values, "row", row.ReferenceId, Place(keys.Groups[row.GroupId], row.TableRowTemplateId));
        }

        return keys;
    }

    private int KeyFor(IEnumerable<int> usedInCopy, string kind, Guid referenceId, string place)
    {
        if (_known.TryGetValue((kind, referenceId), out var known) && !usedInCopy.Contains(known.Key))
        {
            if (string.Equals(known.Place, place, StringComparison.Ordinal))
            {
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
        return key;
    }

    private static string Place(params int[] parts)
    {
        return string.Join('/', parts.Select(part => part.ToString(CultureInfo.InvariantCulture)));
    }
}
