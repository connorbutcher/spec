using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Published.Sheets;

public static class PublishedSheetAssembler
{
    public static PublishedSheet Assemble(
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

            return new PublishedSheet
            {
                Sheet = version.SheetPublicId,
                Version = version.VersionNumber,
                PublishedAtUtc = version.PublishedAtUtc,
                Cells = flat,
                Missing = missing,
            };
        }

        var tables = Tables(selection, structure, shown, values);
        return new PublishedSheet
        {
            Sheet = version.SheetPublicId,
            Version = version.VersionNumber,
            PublishedAtUtc = version.PublishedAtUtc,
            Tables = tables,
            Missing = missing,
        };
    }

    private static ILookup<int, PublishedRow> RowsBySection(
        PublishedSheetSelection selection,
        PublishedStructure structure,
        List<PublishedCellRecord> shown,
        IReadOnlyDictionary<int, object> values,
        Dictionary<int, int> blockOrder)
    {
        return shown
            .GroupBy(cell => cell.RowId)
            .Select(row => new
            {
                First = row.First(),
                Row = new PublishedRow { Id = row.First().RowPublicId, Cells = Cells(row, selection, structure, blockOrder, values) },
            })
            .Where(row => row.Row.Cells.Count > 0)
            .OrderBy(row => row.First.RowOrder)
            .ThenBy(row => row.First.RowId)
            .ToLookup(row => row.First.SectionId, row => row.Row);
    }

    private static List<PublishedTable> Tables(
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
        var rowsBySection = RowsBySection(selection, structure, shown, values, blockOrder);
        var sectionsByParent = structure.Sections
            .OrderBy(section => section.DisplayOrder)
            .ThenBy(section => section.Id)
            .ToLookup(section => (section.TableId, section.ParentSectionId));
        var blocksByTable = structure.ColumnBlocks
            .OrderBy(block => block.DisplayOrder)
            .ThenBy(block => block.Id)
            .ToLookup(block => block.TableId);

        var tables = new List<PublishedTable>();
        foreach (var table in structure.Tables)
        {
            var sections = Sections(table.Id, null, selection, sectionsByParent, rowsBySection);
            if (sections.Count == 0 && !selection.IsEverything)
            {
                continue;
            }

            var blocks = blocksByTable[table.Id]
                .Select(block => new PublishedColumnBlock { Id = block.PublicId, Name = selection.IncludeLabels ? block.Name : null })
                .ToList();
            tables.Add(new PublishedTable
            {
                Id = table.PublicId,
                Title = selection.IncludeLabels ? table.Title : null,
                Sections = sections,
                Columns = blocks.Count == 0 ? null : blocks,
            });
        }

        return tables;
    }

    private static List<PublishedSection> Sections(
        int tableId,
        int? parentSectionId,
        PublishedSheetSelection selection,
        ILookup<(int TableId, int? ParentSectionId), PublishedSectionRecord> sectionsByParent,
        ILookup<int, PublishedRow> rowsBySection)
    {
        var result = new List<PublishedSection>();
        foreach (var section in sectionsByParent[(tableId, parentSectionId)])
        {
            var rows = rowsBySection[section.Id].ToList();
            var children = Sections(tableId, section.Id, selection, sectionsByParent, rowsBySection);

            // A selection only shows the sections that lead to something the caller asked for.
            if (rows.Count == 0 && children.Count == 0 && !selection.IsEverything)
            {
                continue;
            }

            result.Add(new PublishedSection
            {
                Id = section.PublicId,
                Name = selection.IncludeLabels ? section.Name : null,
                Rows = rows,
                Sections = children,
            });
        }

        return result;
    }

    private static List<PublishedCell> Cells(
        IEnumerable<PublishedCellRecord> row,
        PublishedSheetSelection selection,
        PublishedStructure structure,
        Dictionary<int, int> blockOrder,
        IReadOnlyDictionary<int, object> values)
    {
        var result = new List<PublishedCell>();

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

            var column = cell.ColumnBlockId is { } blockId ? structure.ColumnBlock(blockId)?.PublicId : null;
            result.Add(new PublishedCell
            {
                Id = cell.CellPublicId,
                Value = value,
                Column = column,
                Caption = selection.IncludeLabels ? cell.Caption : null,
            });
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
