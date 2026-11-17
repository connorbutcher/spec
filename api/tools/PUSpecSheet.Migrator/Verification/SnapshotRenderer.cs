using System.Globalization;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Verification;

/// <summary>
/// Writes a <see cref="SheetSnapshot"/> out as lines of text that say only what a person would see: the tables,
/// groups, rows and cell contents in display order, with their templates and ReferenceIDs, and no ids or order
/// numbers. Two snapshots that render the same show the same sheet, however differently they are stored. Empty cells
/// aren't written.
/// </summary>
internal static class SnapshotRenderer
{
    public static IReadOnlyList<string> Render(SheetSnapshot snapshot)
    {
        var lines = new List<string>();
        var tables = snapshot.Tables.Select(table =>
        {
            var tableLines = new List<string> { $"table {table.ReferenceId} template {table.TableTemplateId} {SnapshotJson.Text(table.Title)}" };
            if (SnapshotJson.Of(table.ConfigurationOverride) is { } configuration)
            {
                tableLines.Add($"  configuration {configuration}");
            }

            if (SnapshotJson.Of(table.WidgetValues) is { } widgets)
            {
                tableLines.Add($"  widgets {widgets}");
            }

            if (SnapshotJson.Of(table.Notes) is { } notes)
            {
                tableLines.Add($"  notes {notes}");
            }

            tableLines.AddRange(Groups(snapshot, table.Id));
            return (table.DisplayOrder, table.TableTemplateId, tableLines);
        });
        lines.AddRange(Ordered(tables));

        foreach (var (propertyId, value) in snapshot.Properties.OrderBy(pair => pair.Key))
        {
            lines.Add($"property {propertyId} = {SnapshotJson.Text(value)}");
        }

        foreach (var link in snapshot.LinkedTables.OrderBy(link => link.DisplayOrder).ThenBy(link => link.TableReferenceId))
        {
            lines.Add(string.Create(CultureInfo.InvariantCulture, $"linked table {link.TableReferenceId} by user {link.LinkedByUserId} at {link.LinkedDateTime:O}"));
        }

        foreach (var reference in (snapshot.ExternalReferences ?? []).OrderBy(reference => reference.ReferenceId))
        {
            lines.Add($"external reference {reference.ReferenceId} {reference.Signature}");
        }

        return lines;
    }

    private static List<string> Groups(SheetSnapshot snapshot, int tableId)
    {
        var groups = snapshot.Groups
            .Where(group => group.TableId == tableId)
            .Select(group =>
            {
                var lines = new List<string> { $"  group {group.ReferenceId} template {group.TableGroupTemplateId}" };
                if (SnapshotJson.Of(group.WidgetValues) is { } widgets)
                {
                    lines.Add($"    widgets {widgets}");
                }

                if (group.LinkedGroupReferenceId is { } linked)
                {
                    lines.Add($"    linked to group {linked}");
                }

                lines.AddRange(Rows(snapshot, group.Id));
                return (group.DisplayOrder, group.TableGroupTemplateId, lines);
            });

        return Ordered(groups);
    }

    private static List<string> Rows(SheetSnapshot snapshot, int groupId)
    {
        var rows = snapshot.Rows
            .Where(row => row.GroupId == groupId)
            .Select(row =>
            {
                var lines = new List<string> { $"    row {row.ReferenceId} template {row.TableRowTemplateId}" };
                lines.AddRange(snapshot.CellsOf(row.Id)
                    .Where(cell => !cell.Content.IsEmpty)
                    .Select(cell => $"      cell {cell.ReferenceId} template {cell.TableCellTemplateId}: {cell.Content.Signature}")
                    .Order(StringComparer.Ordinal));
                return (row.DisplayOrder, row.TableRowTemplateId, lines);
            });

        return Ordered(rows);
    }

    /// <summary>
    /// Siblings in display order; ties, which the stored order can't settle, by template then by what they hold.
    /// </summary>
    private static List<string> Ordered(IEnumerable<(int DisplayOrder, int TemplateId, List<string> Lines)> siblings)
    {
        return siblings
            .OrderBy(sibling => sibling.DisplayOrder)
            .ThenBy(sibling => sibling.TemplateId)
            .ThenBy(sibling => string.Join('\n', sibling.Lines), StringComparer.Ordinal)
            .SelectMany(sibling => sibling.Lines)
            .ToList();
    }
}
