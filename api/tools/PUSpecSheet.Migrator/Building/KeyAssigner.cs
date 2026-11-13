using System.Globalization;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// Gives the items of each copy their keys, copy by copy in order, by matching them with the copy before (see
/// <see cref="SiblingMatcher"/>). Items are only matched with items of the same template under the same matched
/// parent, as neither can change: tables by template version, then each table's column blocks, its sections top
/// down, and each section's rows. Rows are matched on their contents, tables on their titles, the rest on order.
/// An item with no partner, and everything inside it, gets a new key.
/// </summary>
internal sealed class KeyAssigner
{
    private int nextKey = 1;
    private SheetSnapshot previous = new();
    private SnapshotKeys previousKeys = new();

    public SnapshotKeys Assign(SheetSnapshot current)
    {
        var keys = new SnapshotKeys();

        AssignTables(current, keys);
        foreach (var table in current.Tables)
        {
            AssignColumnBlocks(current, keys, table);
            AssignSections(current, keys, table, parent: null);
        }

        foreach (var section in current.Sections)
        {
            AssignRows(current, keys, section);
        }

        previous = current;
        previousKeys = keys;
        return keys;
    }

    /// <summary>A row's contents, by its cells' keys, for matching it with its previous self.</summary>
    public static string RowSignature(SheetSnapshot snapshot, SnapshotKeys keys, int rowId)
    {
        var cells = snapshot.CellsOf(rowId)
            .Where(cell => !cell.Content.IsEmpty)
            .Select(cell => $"{cell.TemplateCellId}@{BlockKey(keys, cell.ColumnBlockId)}={cell.Content.Signature}")
            .Order(StringComparer.Ordinal);
        return string.Join('\n', cells);
    }

    private static string BlockKey(SnapshotKeys keys, int? columnBlockId)
    {
        return columnBlockId is { } id && keys.ColumnBlocks.TryGetValue(id, out var key) ? key.ToString(CultureInfo.InvariantCulture) : "-";
    }

    private void AssignTables(SheetSnapshot current, SnapshotKeys keys)
    {
        foreach (var group in current.Tables.GroupBy(table => table.TableTemplateVersionId))
        {
            var before = previous.Tables
                .Where(table => table.TableTemplateVersionId == group.Key)
                .OrderBy(table => table.DisplayOrder)
                .ThenBy(table => table.Id)
                .ToList();
            var now = group.OrderBy(table => table.DisplayOrder).ThenBy(table => table.Id).ToList();

            Pair(
                before.Select(table => (previousKeys.Tables[table.Id], table.Title ?? string.Empty)).ToList(),
                now.Select(table => (table.Id, table.Title ?? string.Empty)).ToList(),
                keys.Tables);
        }
    }

    private void AssignColumnBlocks(SheetSnapshot current, SnapshotKeys keys, SnapshotTable table)
    {
        var tableKey = keys.Tables[table.Id];
        foreach (var group in current.ColumnBlocks.Where(block => block.TableId == table.Id).GroupBy(block => block.TemplateColumnBlockId))
        {
            var before = previous.ColumnBlocks
                .Where(block => block.TemplateColumnBlockId == group.Key && previousKeys.Tables.GetValueOrDefault(block.TableId) == tableKey)
                .OrderBy(block => block.DisplayOrder)
                .ThenBy(block => block.Id)
                .ToList();
            var now = group.OrderBy(block => block.DisplayOrder).ThenBy(block => block.Id).ToList();

            Pair(
                before.Select(block => (previousKeys.ColumnBlocks[block.Id], string.Empty)).ToList(),
                now.Select(block => (block.Id, string.Empty)).ToList(),
                keys.ColumnBlocks);
        }
    }

    private void AssignSections(SheetSnapshot current, SnapshotKeys keys, SnapshotTable table, SnapshotSection? parent)
    {
        var tableKey = keys.Tables[table.Id];
        int? parentKey = parent is null ? null : keys.Sections[parent.Id];
        var children = current.Sections
            .Where(section => section.TableId == table.Id && section.ParentSectionId == parent?.Id)
            .ToList();

        foreach (var group in children.GroupBy(section => section.TemplateSectionId))
        {
            var before = previous.Sections
                .Where(section => section.TemplateSectionId == group.Key
                    && previousKeys.Tables.GetValueOrDefault(section.TableId) == tableKey
                    && ParentKeyBefore(section) == parentKey)
                .OrderBy(section => section.DisplayOrder)
                .ThenBy(section => section.Id)
                .ToList();
            var now = group.OrderBy(section => section.DisplayOrder).ThenBy(section => section.Id).ToList();

            Pair(
                before.Select(section => (previousKeys.Sections[section.Id], string.Empty)).ToList(),
                now.Select(section => (section.Id, string.Empty)).ToList(),
                keys.Sections);
        }

        foreach (var child in children)
        {
            AssignSections(current, keys, table, child);
        }
    }

    private void AssignRows(SheetSnapshot current, SnapshotKeys keys, SnapshotSection section)
    {
        var sectionKey = keys.Sections[section.Id];
        foreach (var group in current.Rows.Where(row => row.SectionId == section.Id).GroupBy(row => row.TemplateRowId))
        {
            var before = previous.Rows
                .Where(row => row.TemplateRowId == group.Key && previousKeys.Sections.GetValueOrDefault(row.SectionId) == sectionKey)
                .OrderBy(row => row.DisplayOrder)
                .ThenBy(row => row.Id)
                .ToList();
            var now = group.OrderBy(row => row.DisplayOrder).ThenBy(row => row.Id).ToList();

            Pair(
                before.Select(row => (previousKeys.Rows[row.Id], RowSignature(previous, previousKeys, row.Id))).ToList(),
                now.Select(row => (row.Id, RowSignature(current, keys, row.Id))).ToList(),
                keys.Rows);
        }
    }

    private int? ParentKeyBefore(SnapshotSection section)
    {
        return section.ParentSectionId is { } parentId ? previousKeys.Sections[parentId] : null;
    }

    /// <summary>Gives each current item its partner's key, or a new one.</summary>
    private void Pair(List<(int Key, string Signature)> before, List<(int Id, string Signature)> now, Dictionary<int, int> keys)
    {
        var partners = SiblingMatcher.Match(
            before.Select(item => item.Signature).ToList(),
            now.Select(item => item.Signature).ToList());

        for (var index = 0; index < now.Count; index++)
        {
            keys[now[index].Id] = partners[index] >= 0 ? before[partners[index]].Key : nextKey++;
        }
    }
}
