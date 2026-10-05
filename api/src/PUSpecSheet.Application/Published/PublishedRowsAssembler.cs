using System.Globalization;
using PUSpecSheet.Contracts.Published;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Builds rows by identifier from what the queries returned. Each row has its own values and, on a table
/// with column blocks, its values in each block under the block's name: the value at the top of the block
/// in the table's header, such as a part number.
/// </summary>
public static class PublishedRowsAssembler
{
    public static PublishedRowsDto Assemble(
        ResolvedSheetVersion version,
        PublishedRowsSelection selection,
        PublishedStructure structure,
        IReadOnlyList<PublishedLookupCellRecord> cells,
        IReadOnlyList<PublishedLookupCellRecord> headerCells,
        IReadOnlyDictionary<int, object> values)
    {
        var sections = structure.Sections.ToDictionary(section => section.Id);
        var sectionOrder = PublishedLookupAssembler.SectionOrder(sections.Values);
        var tableOrder = structure.Tables
            .Select((table, index) => (table.Id, index))
            .ToDictionary(entry => entry.Id, entry => entry.index);
        var blocks = structure.ColumnBlocks
            .OrderBy(block => block.DisplayOrder)
            .ThenBy(block => block.Id)
            .ToList();
        var names = BlockNames(blocks, headerCells.Where(cell => sections.ContainsKey(cell.SectionId)), values);

        var kept = blocks
            .Where(block => selection.Columns.Count == 0
                || selection.Columns.Contains(names[block.Id])
                || selection.Columns.Contains(block.PublicId.ToString()))
            .ToList();

        var rows = new Dictionary<Guid, PublishedRowValuesDto>();
        var shown = cells
            .Where(cell => sections.ContainsKey(cell.SectionId)
                && (cell.ColumnBlockId is not { } blockId || structure.ColumnBlock(blockId) is not null))
            .OrderBy(cell => tableOrder[sections[cell.SectionId].TableId])
            .ThenBy(cell => sectionOrder[cell.SectionId])
            .ThenBy(cell => cell.RowOrder)
            .ThenBy(cell => cell.RowId)
            .ThenBy(cell => cell.Column)
            .ThenBy(cell => cell.CellId);

        foreach (var row in shown.GroupBy(cell => cell.RowId))
        {
            var valueCells = row.Where(cell => cell.Kind.StoresValue()).ToList();

            // A row of headings holds no data. It is only left out when nobody asked for it by name.
            if (valueCells.Count == 0 && selection.IsEverything)
            {
                continue;
            }

            var first = row.First();
            var own = ValuesOf(valueCells.Where(cell => cell.ColumnBlockId is null), values);

            Dictionary<string, IReadOnlyList<object?>>? columns = null;
            if (row.Any(cell => cell.ColumnBlockId is not null))
            {
                columns = [];
                foreach (var block in kept)
                {
                    var inBlock = valueCells.Where(cell => cell.ColumnBlockId == block.Id).ToList();
                    if (inBlock.Count > 0)
                    {
                        columns[names[block.Id]] = ValuesOf(inBlock, values);
                    }
                }
            }

            rows[first.RowPublicId] = new PublishedRowValuesDto(sections[first.SectionId].Name, own, columns);
        }

        var missing = selection.Rows.Where(id => !rows.ContainsKey(id)).Order().ToList();
        return new PublishedRowsDto(version.SheetPublicId, version.VersionNumber, version.PublishedAtUtc, rows, missing);
    }

    /// <summary>
    /// What each block is called: the first value in its header cells, top to bottom then left to right.
    /// A block with no such value, or with one an earlier block of its table already has, goes by its
    /// identifier, so no two blocks of a table share a name.
    /// </summary>
    internal static Dictionary<int, string> BlockNames(
        List<PublishedColumnBlockRecord> blocks,
        IEnumerable<PublishedLookupCellRecord> headerCells,
        IReadOnlyDictionary<int, object> values)
    {
        var firstValues = headerCells
            .Where(cell => cell.IsHeader && cell.ColumnBlockId is not null && cell.Kind.StoresValue() && values.ContainsKey(cell.CellId))
            .OrderBy(cell => cell.RowOrder)
            .ThenBy(cell => cell.RowId)
            .ThenBy(cell => cell.Column)
            .GroupBy(cell => cell.ColumnBlockId!.Value)
            .ToDictionary(group => group.Key, group => Convert.ToString(values[group.First().CellId], CultureInfo.InvariantCulture));

        var names = new Dictionary<int, string>();
        var taken = new HashSet<(int TableId, string Name)>();
        foreach (var block in blocks)
        {
            var name = firstValues.GetValueOrDefault(block.Id)?.Trim();
            if (string.IsNullOrEmpty(name) || !taken.Add((block.TableId, name.ToUpperInvariant())))
            {
                name = block.PublicId.ToString();
            }

            names[block.Id] = name;
        }

        return names;
    }

    private static List<object?> ValuesOf(IEnumerable<PublishedLookupCellRecord> cells, IReadOnlyDictionary<int, object> values)
    {
        return cells.Select(cell => values.GetValueOrDefault(cell.CellId)).ToList();
    }
}
