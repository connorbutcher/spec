using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Builds the response from what the queries returned: drops anything inside a removed table, section or
/// column block, puts the rest in display order, and lists what the caller named that wasn't on the sheet.
/// </summary>
public static class PublishedSheetAssembler
{
    public static PublishedSheetDto Assemble(
        ResolvedSheetVersion version,
        PublishedSheetSelection selection,
        PublishedStructure structure,
        IReadOnlyList<PublishedCellRecord> cells,
        IReadOnlyDictionary<int, object> values)
    {
        var shown = cells.Where(structure.Shows).ToList();
        var missing = Missing(selection, structure, shown);

        if (selection.Shape == PublishedSheetShape.Flat)
        {
            var flat = new Dictionary<Guid, object>();
            foreach (var cell in shown)
            {
                if (values.TryGetValue(cell.CellId, out var value))
                {
                    flat[cell.CellPublicId] = value;
                }
            }

            return new PublishedSheetDto(version.SheetPublicId, version.VersionNumber, version.PublishedAtUtc, null, flat, missing);
        }

        var tables = Tables(selection, structure, shown, values);
        return new PublishedSheetDto(version.SheetPublicId, version.VersionNumber, version.PublishedAtUtc, tables, null, missing);
    }

    private static List<PublishedTableDto> Tables(
        PublishedSheetSelection selection,
        PublishedStructure structure,
        List<PublishedCellRecord> shown,
        IReadOnlyDictionary<int, object> values)
    {
        var blockOrder = structure.ColumnBlocks
            .OrderBy(block => block.DisplayOrder)
            .ThenBy(block => block.Id)
            .Select((block, index) => (block.Id, index))
            .ToDictionary(entry => entry.Id, entry => entry.index);
        var rowsBySection = shown
            .GroupBy(cell => cell.RowId)
            .Select(row => new
            {
                First = row.First(),
                Dto = new PublishedRowDto(row.First().RowPublicId, Cells(row, selection, structure, blockOrder, values)),
            })
            .OrderBy(row => row.First.RowOrder)
            .ThenBy(row => row.First.RowId)
            .ToLookup(row => row.First.SectionId, row => row.Dto);
        var sectionsByParent = structure.Sections
            .OrderBy(section => section.DisplayOrder)
            .ThenBy(section => section.Id)
            .ToLookup(section => (section.TableId, section.ParentSectionId));
        var blocksByTable = structure.ColumnBlocks
            .OrderBy(block => block.DisplayOrder)
            .ThenBy(block => block.Id)
            .ToLookup(block => block.TableId);

        var tables = new List<PublishedTableDto>();
        foreach (var table in structure.Tables)
        {
            var sections = Sections(table.Id, null, selection, sectionsByParent, rowsBySection);
            if (sections.Count == 0 && !selection.IsEverything)
            {
                continue;
            }

            var blocks = blocksByTable[table.Id]
                .Select(block => new PublishedColumnBlockDto(block.PublicId, selection.IncludeLabels ? block.Name : null))
                .ToList();
            tables.Add(new PublishedTableDto(
                table.PublicId,
                selection.IncludeLabels ? table.Title : null,
                sections,
                blocks.Count == 0 ? null : blocks));
        }

        return tables;
    }

    private static List<PublishedSectionDto> Sections(
        int tableId,
        int? parentSectionId,
        PublishedSheetSelection selection,
        ILookup<(int TableId, int? ParentSectionId), PublishedSectionRecord> sectionsByParent,
        ILookup<int, PublishedRowDto> rowsBySection)
    {
        var result = new List<PublishedSectionDto>();
        foreach (var section in sectionsByParent[(tableId, parentSectionId)])
        {
            var rows = rowsBySection[section.Id].ToList();
            var children = Sections(tableId, section.Id, selection, sectionsByParent, rowsBySection);

            // A selection only shows the sections that lead to something the caller asked for.
            if (rows.Count == 0 && children.Count == 0 && !selection.IsEverything)
            {
                continue;
            }

            result.Add(new PublishedSectionDto(
                section.PublicId,
                selection.IncludeLabels ? section.Name : null,
                rows,
                children));
        }

        return result;
    }

    private static List<PublishedCellDto> Cells(
        IEnumerable<PublishedCellRecord> row,
        PublishedSheetSelection selection,
        PublishedStructure structure,
        Dictionary<int, int> blockOrder,
        IReadOnlyDictionary<int, object> values)
    {
        var result = new List<PublishedCellDto>();

        // The row's own cells come first, then each column block's cells, left to right.
        var ordered = row
            .OrderBy(cell => cell.ColumnBlockId is { } blockId ? blockOrder[blockId] : -1)
            .ThenBy(cell => cell.Column)
            .ThenBy(cell => cell.CellId);
        foreach (var cell in ordered)
        {
            if (!values.TryGetValue(cell.CellId, out var value))
            {
                continue;
            }

            var block = cell.ColumnBlockId is { } blockId ? structure.ColumnBlock(blockId)?.PublicId : null;
            result.Add(new PublishedCellDto(cell.CellPublicId, value, block, selection.IncludeLabels ? cell.Caption : null));
        }

        return result;
    }

    private static List<Guid> Missing(
        PublishedSheetSelection selection,
        PublishedStructure structure,
        List<PublishedCellRecord> shown)
    {
        if (selection.IsEverything)
        {
            return [];
        }

        var tables = structure.Tables.Select(table => table.PublicId).ToHashSet();
        var sections = structure.Sections.Select(section => section.PublicId).ToHashSet();
        var rows = shown.Select(cell => cell.RowPublicId).ToHashSet();
        var cells = shown.Select(cell => cell.CellPublicId).ToHashSet();

        return
        [
            .. selection.Tables.Where(id => !tables.Contains(id)).Order(),
            .. selection.Sections.Where(id => !sections.Contains(id)).Order(),
            .. selection.Rows.Where(id => !rows.Contains(id)).Order(),
            .. selection.Cells.Where(id => !cells.Contains(id)).Order(),
        ];
    }
}
