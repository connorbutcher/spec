using PUSpecSheet.Contracts.Published;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Builds what a caller receives for one cell a lookup found, as rows by identifier in the same shape as
/// <see cref="PublishedRowsAssembler"/>. A cell in a column block brings the block's column: every row of
/// the table with its own values and the block's values under the block's name. A cell among a row's own
/// cells brings its rows: those of its section and sub-sections, or of the whole table when the cell is in
/// the header. Rows that hold nothing besides the cell that was found are left out.
/// </summary>
public static class PublishedLookupAssembler
{
    /// <summary>Whether the cell's section, and its column block if it has one, were on the sheet at the version.</summary>
    public static bool IsOnSheet(PublishedLookupHit hit, PublishedStructure structure)
    {
        return structure.Sections.Any(section => section.Id == hit.SectionId)
            && (hit.ColumnBlockId is not { } blockId || structure.ColumnBlock(blockId) is not null);
    }

    /// <summary>
    /// The sections whose rows belong to the match: the cell's section and everything beneath it. Null
    /// means every row of the table, which is what a cell in a column block or in the header brings.
    /// </summary>
    public static IReadOnlyList<int>? ScopeSections(PublishedLookupHit hit, PublishedStructure structure)
    {
        if (hit.ColumnBlockId is not null || hit.IsHeader)
        {
            return null;
        }

        var children = structure.Sections
            .Where(section => section.ParentSectionId is not null)
            .ToLookup(section => section.ParentSectionId!.Value, section => section.Id);
        var result = new List<int>();
        var pending = new Stack<int>([hit.SectionId]);
        while (pending.TryPop(out var sectionId))
        {
            result.Add(sectionId);
            foreach (var child in children[sectionId])
            {
                pending.Push(child);
            }
        }

        return result;
    }

    public static PublishedLookupMatchDto Assemble(
        PublishedLookupHit hit,
        ResolvedSheetVersion version,
        PublishedStructure structure,
        IReadOnlyList<PublishedLookupCellRecord> cells,
        IReadOnlyDictionary<int, object> values)
    {
        var table = structure.Tables.First(candidate => candidate.Id == hit.TableId);
        var sections = structure.Sections
            .Where(section => section.TableId == hit.TableId)
            .ToDictionary(section => section.Id);
        var shown = ShownCells(cells, sections, structure);
        var blocks = structure.ColumnBlocks
            .Where(block => block.TableId == hit.TableId)
            .OrderBy(block => block.DisplayOrder)
            .ThenBy(block => block.Id)
            .ToList();
        var names = PublishedRowsAssembler.BlockNames(blocks, shown, values);

        // A cell in a block brings that block alone; a cell in a row brings the row across every block.
        var kept = hit.ColumnBlockId is { } matchedBlockId
            ? blocks.Where(block => block.Id == matchedBlockId).ToList()
            : blocks;
        var scope = ScopeSections(hit, structure)?.ToHashSet();
        var rows = new Dictionary<Guid, PublishedRowValuesDto>();
        foreach (var row in shown.Where(cell => scope is null || scope.Contains(cell.SectionId)).GroupBy(cell => cell.RowId))
        {
            if (RowValues(hit, row, kept, names, sections, values) is { } rowValues)
            {
                rows[row.First().RowPublicId] = rowValues;
            }
        }

        return new PublishedLookupMatchDto(
            version.SheetPublicId,
            hit.PhaseCode,
            hit.SheetTypeId,
            version.VersionNumber,
            version.PublishedAtUtc,
            table.PublicId,
            table.Title,
            hit.ColumnBlockId is { } namedBlockId ? names[namedBlockId] : null,
            Headings(hit, shown),
            rows);
    }

    /// <summary>Each section's place when the table is read top to bottom, sub-sections after their parent's rows.</summary>
    internal static Dictionary<int, int> SectionOrder(IEnumerable<PublishedSectionRecord> sections)
    {
        var byParent = sections
            .OrderBy(section => section.DisplayOrder)
            .ThenBy(section => section.Id)
            .ToLookup(section => section.ParentSectionId);
        var order = new Dictionary<int, int>();
        var pending = new Stack<PublishedSectionRecord>(byParent[null].Reverse());
        while (pending.TryPop(out var section))
        {
            if (!order.TryAdd(section.Id, order.Count))
            {
                continue;
            }

            foreach (var child in byParent[section.Id].Reverse())
            {
                pending.Push(child);
            }
        }

        return order;
    }

    /// <summary>Everything that was on the sheet, top to bottom and then left to right.</summary>
    private static List<PublishedLookupCellRecord> ShownCells(
        IReadOnlyList<PublishedLookupCellRecord> cells,
        Dictionary<int, PublishedSectionRecord> sections,
        PublishedStructure structure)
    {
        var sectionOrder = SectionOrder(sections.Values);
        return cells
            .Where(cell => sections.ContainsKey(cell.SectionId)
                && (cell.ColumnBlockId is not { } blockId || structure.ColumnBlock(blockId) is not null))
            .OrderBy(cell => sectionOrder[cell.SectionId])
            .ThenBy(cell => cell.RowOrder)
            .ThenBy(cell => cell.RowId)
            .ThenBy(cell => cell.Column)
            .ThenBy(cell => cell.CellId)
            .ToList();
    }

    /// <summary>The headings over the values: the block's for a cell in a block, the table's own otherwise.</summary>
    private static List<string> Headings(PublishedLookupHit hit, List<PublishedLookupCellRecord> shown)
    {
        return shown
            .Where(cell => cell.IsHeader
                && !cell.Kind.StoresValue()
                && !string.IsNullOrWhiteSpace(cell.Caption)
                && cell.ColumnBlockId == hit.ColumnBlockId)
            .Select(cell => cell.Caption!)
            .ToList();
    }

    /// <summary>A row's values, or null when it holds nothing besides the cell that was found.</summary>
    private static PublishedRowValuesDto? RowValues(
        PublishedLookupHit hit,
        IGrouping<int, PublishedLookupCellRecord> row,
        List<PublishedColumnBlockRecord> kept,
        IReadOnlyDictionary<int, string> names,
        Dictionary<int, PublishedSectionRecord> sections,
        IReadOnlyDictionary<int, object> values)
    {
        var valueCells = row
            .Where(cell => cell.Kind.StoresValue()
                && (cell.ColumnBlockId is not { } blockId || kept.Any(block => block.Id == blockId)))
            .ToList();
        if (!valueCells.Any(cell => cell.CellId != hit.CellId && values.ContainsKey(cell.CellId)))
        {
            return null;
        }

        var own = ValuesOf(valueCells.Where(cell => cell.ColumnBlockId is null), values);
        var columns = row.Any(cell => cell.ColumnBlockId is not null)
            ? PublishedRowsAssembler.BlockColumns(valueCells, kept, names, values)
            : null;
        return new PublishedRowValuesDto(sections[row.First().SectionId].Name, own, columns);
    }

    private static List<object?> ValuesOf(IEnumerable<PublishedLookupCellRecord> cells, IReadOnlyDictionary<int, object> values)
    {
        return cells.Select(cell => values.GetValueOrDefault(cell.CellId)).ToList();
    }
}
