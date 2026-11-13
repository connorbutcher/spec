using System.Text.Json;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Verification;

/// <summary>
/// Writes a <see cref="SheetSnapshot"/> out as lines of text that say only what a person would see: the tables,
/// column blocks, sections, rows and cell contents in display order, by template, with no ids. Two snapshots
/// that render the same show the same sheet, however differently they are stored. Column blocks are referred to
/// by their place in their table. Empty cells aren't written.
/// </summary>
internal static class SnapshotRenderer
{
    public static IReadOnlyList<string> Render(SheetSnapshot snapshot)
    {
        var lines = new List<string>();
        foreach (var table in snapshot.Tables.OrderBy(table => table.DisplayOrder).ThenBy(table => table.TableTemplateVersionId).ThenBy(table => table.Title, StringComparer.Ordinal))
        {
            lines.Add($"table template-version {table.TableTemplateVersionId} {JsonSerializer.Serialize(table.Title)}");

            var blocks = snapshot.ColumnBlocks
                .Where(block => block.TableId == table.Id)
                .OrderBy(block => block.DisplayOrder)
                .ThenBy(block => block.TemplateColumnBlockId)
                .ToList();
            var blockPlaces = new Dictionary<int, int>();
            for (var place = 0; place < blocks.Count; place++)
            {
                blockPlaces[blocks[place].Id] = place + 1;
                lines.Add($"  column block {place + 1}: template {blocks[place].TemplateColumnBlockId}");
            }

            lines.AddRange(Sections(snapshot, table.Id, null, blockPlaces, "  "));
        }

        return lines;
    }

    private static List<string> Sections(SheetSnapshot snapshot, int tableId, int? parentId, Dictionary<int, int> blockPlaces, string indent)
    {
        var rendered = snapshot.Sections
            .Where(section => section.TableId == tableId && section.ParentSectionId == parentId)
            .Select(section =>
            {
                var lines = new List<string> { $"{indent}section template {section.TemplateSectionId}" };
                lines.AddRange(Rows(snapshot, section.Id, blockPlaces, indent + "  "));
                lines.AddRange(Sections(snapshot, tableId, section.Id, blockPlaces, indent + "  "));
                return (section.DisplayOrder, section.TemplateSectionId, lines);
            });

        return Ordered(rendered);
    }

    private static List<string> Rows(SheetSnapshot snapshot, int sectionId, Dictionary<int, int> blockPlaces, string indent)
    {
        var rendered = snapshot.Rows
            .Where(row => row.SectionId == sectionId)
            .Select(row =>
            {
                var lines = new List<string> { $"{indent}row template {row.TemplateRowId}" };
                lines.AddRange(snapshot.CellsOf(row.Id)
                    .Where(cell => !cell.Content.IsEmpty)
                    .Select(cell => $"{indent}  cell template {cell.TemplateCellId} block {BlockPlace(blockPlaces, cell.ColumnBlockId)}: {cell.Content.Signature}")
                    .Order(StringComparer.Ordinal));
                return (row.DisplayOrder, row.TemplateRowId, lines);
            });

        return Ordered(rendered);
    }

    /// <summary>Siblings in display order; ties, which the stored order can't settle, by template then by what they hold.</summary>
    private static List<string> Ordered(IEnumerable<(int DisplayOrder, int TemplateId, List<string> Lines)> siblings)
    {
        return siblings
            .OrderBy(sibling => sibling.DisplayOrder)
            .ThenBy(sibling => sibling.TemplateId)
            .ThenBy(sibling => string.Join('\n', sibling.Lines), StringComparer.Ordinal)
            .SelectMany(sibling => sibling.Lines)
            .ToList();
    }

    private static string BlockPlace(Dictionary<int, int> blockPlaces, int? columnBlockId)
    {
        if (columnBlockId is not { } id)
        {
            return "-";
        }

        return blockPlaces.TryGetValue(id, out var place) ? $"{place}" : "?";
    }
}
