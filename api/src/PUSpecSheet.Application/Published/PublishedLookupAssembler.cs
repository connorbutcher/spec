using PUSpecSheet.Contracts.Published;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Builds what a caller receives for one cell a lookup found. A cell in a column block brings the block's
/// column: every row of the table with its description and the block's values. A cell among a row's own
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
        var sectionOrder = SectionOrder(sections.Values);
        var scope = ScopeSections(hit, structure)?.ToHashSet();

        // Everything that was on the sheet, top to bottom and then left to right.
        var shown = cells
            .Where(cell => sections.ContainsKey(cell.SectionId)
                && (cell.ColumnBlockId is not { } blockId || structure.ColumnBlock(blockId) is not null))
            .OrderBy(cell => sectionOrder[cell.SectionId])
            .ThenBy(cell => cell.RowOrder)
            .ThenBy(cell => cell.RowId)
            .ThenBy(cell => cell.Column)
            .ThenBy(cell => cell.CellId)
            .ToList();

        // The headings over the values: the block's for a cell in a block, the table's own otherwise.
        var columns = shown
            .Where(cell => cell.IsHeader
                && !cell.Kind.StoresValue()
                && !string.IsNullOrWhiteSpace(cell.Caption)
                && cell.ColumnBlockId == hit.ColumnBlockId)
            .Select(cell => cell.Caption!)
            .ToList();

        var blocks = hit.ColumnBlockId is null ? BlocksIn(shown, structure) : [];

        var rows = new List<PublishedLookupRowDto>();
        foreach (var row in shown.Where(cell => scope is null || scope.Contains(cell.SectionId)).GroupBy(cell => cell.RowId))
        {
            var valueCells = row.Where(cell => cell.Kind.StoresValue()).ToList();
            if (!valueCells.Any(cell => cell.CellId != hit.CellId && values.ContainsKey(cell.CellId)))
            {
                continue;
            }

            var first = row.First();
            var section = sections[first.SectionId].Name;
            var own = ValuesOf(valueCells.Where(cell => cell.ColumnBlockId is null), values);

            if (hit.ColumnBlockId is { } blockId)
            {
                var inBlock = ValuesOf(valueCells.Where(cell => cell.ColumnBlockId == blockId), values);
                rows.Add(new PublishedLookupRowDto(first.RowPublicId, section, Description(own), inBlock, null));
                continue;
            }

            var perBlock = blocks
                .Select(block => new PublishedLookupBlockValuesDto(
                    block.PublicId,
                    ValuesOf(valueCells.Where(cell => cell.ColumnBlockId == block.Id), values)))
                .Where(entry => entry.Values.Count > 0)
                .ToList();
            rows.Add(new PublishedLookupRowDto(first.RowPublicId, section, null, own, perBlock.Count == 0 ? null : perBlock));
        }

        return new PublishedLookupMatchDto(
            version.SheetPublicId,
            hit.PhaseCode,
            hit.SheetTypeId,
            version.VersionNumber,
            version.PublishedAtUtc,
            table.PublicId,
            table.Title,
            hit.ColumnBlockId is { } matchedBlockId ? structure.ColumnBlock(matchedBlockId)!.PublicId : null,
            columns,
            blocks.Count == 0 ? null : blocks.Select(block => BlockDto(block, shown, values)).ToList(),
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

    /// <summary>The column blocks the cells sit in, left to right.</summary>
    private static List<PublishedColumnBlockRecord> BlocksIn(List<PublishedLookupCellRecord> shown, PublishedStructure structure)
    {
        return shown
            .Where(cell => cell.ColumnBlockId is not null)
            .Select(cell => cell.ColumnBlockId!.Value)
            .Distinct()
            .Select(blockId => structure.ColumnBlock(blockId)!)
            .OrderBy(block => block.DisplayOrder)
            .ThenBy(block => block.Id)
            .ToList();
    }

    private static PublishedLookupBlockDto BlockDto(
        PublishedColumnBlockRecord block,
        List<PublishedLookupCellRecord> shown,
        IReadOnlyDictionary<int, object> values)
    {
        var keys = new Dictionary<string, object>();
        foreach (var cell in shown.Where(cell => cell.ColumnBlockId == block.Id && cell.LookupKey is not null))
        {
            if (values.TryGetValue(cell.CellId, out var value))
            {
                keys.TryAdd(cell.LookupKey!, value);
            }
        }

        return new PublishedLookupBlockDto(block.PublicId, keys.Count == 0 ? null : keys);
    }

    private static List<object?> ValuesOf(IEnumerable<PublishedLookupCellRecord> cells, IReadOnlyDictionary<int, object> values)
    {
        return cells.Select(cell => values.GetValueOrDefault(cell.CellId)).ToList();
    }

    /// <summary>A row's own cells beside a block: the one value, the list when there are several, or nothing.</summary>
    private static object? Description(List<object?> own)
    {
        if (own.All(value => value is null))
        {
            return null;
        }

        return own.Count == 1 ? own[0] : own;
    }
}
