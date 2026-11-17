using System.Globalization;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Building;

// Gives each item in a copy a key that follows it from one copy to the next: its legacy ReferenceID, which a copy
// keeps when it is made from another. An item whose ReferenceID turns up under another parent or template than before
// is a new item, since the new layout can't move an item or change its template; the warning says so.
internal sealed class KeyAssigner
{
    private readonly Dictionary<(string Kind, Guid PublicId), (int Key, string Place)> _known = [];
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
            keys.Tables[table.Id] = KeyFor(keys.Tables.Values, "table", table.PublicId, Place(table.TableTemplateVersionId));
        }

        foreach (var block in copy.ColumnBlocks)
        {
            keys.ColumnBlocks[block.Id] = KeyFor(keys.ColumnBlocks.Values, "column block", block.PublicId, Place(keys.Tables[block.TableId], block.TemplateColumnBlockId));
        }

        foreach (var section in ParentsFirst(copy.Sections))
        {
            int? parentKey = section.ParentSectionId is { } parentId ? keys.Sections[parentId] : null;
            keys.Sections[section.Id] = KeyFor(keys.Sections.Values, "section", section.PublicId, Place(keys.Tables[section.TableId], parentKey, section.TemplateSectionId));
        }

        foreach (var row in copy.Rows)
        {
            keys.Rows[row.Id] = KeyFor(keys.Rows.Values, "row", row.PublicId, Place(keys.Sections[row.SectionId], row.TemplateRowId));
        }

        return keys;
    }

    public static List<SnapshotSection> ParentsFirst(IReadOnlyList<SnapshotSection> all)
    {
        var byId = all.ToDictionary(section => section.Id);
        var depths = new Dictionary<int, int>();

        int Depth(SnapshotSection section)
        {
            if (!depths.TryGetValue(section.Id, out var depth))
            {
                depth = section.ParentSectionId is { } parentId ? Depth(byId[parentId]) + 1 : 0;
                depths[section.Id] = depth;
            }

            return depth;
        }

        return all.OrderBy(Depth).ToList();
    }

    private int KeyFor(IEnumerable<int> usedInCopy, string kind, Guid publicId, string place)
    {
        if (_known.TryGetValue((kind, publicId), out var known) && !usedInCopy.Contains(known.Key))
        {
            if (string.Equals(known.Place, place, StringComparison.Ordinal))
            {
                return known.Key;
            }

            _warnings.Add($"The {kind} {publicId} moved to another parent or template, which the new layout can't do; it is removed from where it was and added again.");
        }
        else if (_known.ContainsKey((kind, publicId)))
        {
            _warnings.Add($"The ReferenceID {publicId} is on more than one {kind} in one copy; the second is treated as a new {kind}.");
        }

        var key = _nextKey++;
        _known[(kind, publicId)] = (key, place);
        return key;
    }

    private static string Place(params int?[] parts)
    {
        return string.Join('/', parts.Select(part => part?.ToString(CultureInfo.InvariantCulture) ?? "-"));
    }
}
