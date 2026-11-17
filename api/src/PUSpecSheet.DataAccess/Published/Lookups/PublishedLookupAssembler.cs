using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Published.Rows;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Published.Lookups;

public static class PublishedLookupAssembler
{
    public static bool IsOnSheet(PublishedLookupHit hit, PublishedStructure structure)
    {
        return structure.Sections.Any(section => section.Id == hit.SectionId)
            && (hit.ColumnBlockId is not { } blockId || structure.ColumnBlock(blockId) is not null);
    }

    // Null means every row of the table: a cell in a column block, or in the header, belongs to all of them.
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

    public static PublishedLookupMatch Assemble(
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
        var rows = new Dictionary<Guid, PublishedRowValues>();
        foreach (var row in shown.Where(cell => scope is null || scope.Contains(cell.SectionId)).GroupBy(cell => cell.RowId))
        {
            if (RowValues(hit, row, kept, names, sections, values) is { } rowValues)
            {
                rows[row.First().RowPublicId] = rowValues;
            }
        }

        return new PublishedLookupMatch
        {
            Sheet = version.SheetPublicId,
            Phase = hit.PhaseCode,
            SheetType = hit.SheetTypeId,
            Version = version.VersionNumber,
            PublishedAtUtc = version.PublishedAtUtc,
            Table = table.PublicId,
            Title = table.Title,
            Column = hit.ColumnBlockId is { } namedBlockId ? names[namedBlockId] : null,
            Headings = Headings(hit, shown),
            Rows = rows,
        };
    }

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

    private static PublishedRowValues? RowValues(
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
        return new PublishedRowValues { Section = sections[row.First().SectionId].Name, Values = own, Columns = columns };
    }

    private static List<object?> ValuesOf(IEnumerable<PublishedLookupCellRecord> cells, IReadOnlyDictionary<int, object> values)
    {
        return cells.Select(cell => values.GetValueOrDefault(cell.CellId)).ToList();
    }
}
