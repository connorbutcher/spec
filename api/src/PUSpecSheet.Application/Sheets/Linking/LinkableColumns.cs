using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Linking;

/// <summary>
/// Works out which columns of a table a linked dropdown can take its choices from, and what to call each:
/// the cell's caption, else its lookup key, else the heading above it, else its cell type.
/// </summary>
internal static class LinkableColumns
{
    /// <param name="sections">The sections of one template version.</param>
    /// <param name="rows">The rows of those sections, with their cells.</param>
    /// <param name="cellTypes">Every cell type, by id.</param>
    public static List<SheetLinkableColumnDto> For(
        IReadOnlyCollection<TemplateSection> sections,
        IReadOnlyCollection<TemplateRow> rows,
        IReadOnlyDictionary<int, CellType> cellTypes)
    {
        var sectionsById = sections.ToDictionary(section => section.Id);
        var ordered = rows
            .Where(row => sectionsById.ContainsKey(row.TemplateSectionId))
            .OrderBy(row => sectionsById[row.TemplateSectionId].Role == SectionRole.Header ? 0 : 1)
            .ThenBy(row => sectionsById[row.TemplateSectionId].DisplayOrder)
            .ThenBy(row => row.TemplateSectionId)
            .ThenBy(row => row.DisplayOrder)
            .ThenBy(row => row.Id)
            .ToList();

        var headings = Headings(ordered, sectionsById, cellTypes);
        var columns = new List<(TemplateCell Cell, string Label, string Section)>();
        foreach (var row in ordered)
        {
            foreach (var cell in row.Cells.OrderBy(cell => cell.TemplateColumnBlockId ?? 0).ThenBy(cell => cell.Column).ThenBy(cell => cell.Id))
            {
                if (!cellTypes.TryGetValue(cell.CellTypeId, out var cellType) || !cellType.Kind.CanBeLinkedTo())
                {
                    continue;
                }

                var label = FirstText(cell.Caption, cell.LookupKey, headings.GetValueOrDefault((cell.TemplateColumnBlockId, cell.Column))) ?? cellType.Name;
                columns.Add((cell, label, sectionsById[row.TemplateSectionId].Name));
            }
        }

        // Two columns that would read the same are told apart by their section.
        var repeated = columns
            .GroupBy(column => column.Label, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return columns
            .Select(column => new SheetLinkableColumnDto(
                column.Cell.Id,
                repeated.Contains(column.Label) ? $"{column.Label} ({column.Section})" : column.Label))
            .ToList();
    }

    /// <summary>The header's heading text over each column, by column block and column.</summary>
    private static Dictionary<(int? BlockId, int Column), string> Headings(
        List<TemplateRow> rows,
        Dictionary<int, TemplateSection> sectionsById,
        IReadOnlyDictionary<int, CellType> cellTypes)
    {
        var headings = new Dictionary<(int? BlockId, int Column), string>();
        foreach (var cell in rows
            .Where(row => sectionsById[row.TemplateSectionId].Role == SectionRole.Header)
            .SelectMany(row => row.Cells))
        {
            if (string.IsNullOrWhiteSpace(cell.Caption)
                || !cellTypes.TryGetValue(cell.CellTypeId, out var cellType)
                || cellType.Kind != CellKind.Heading)
            {
                continue;
            }

            for (var column = cell.Column; column < cell.Column + cell.ColumnSpan; column++)
            {
                // The lowest heading over a column is the one nearest the values.
                headings[(cell.TemplateColumnBlockId, column)] = cell.Caption.Trim();
            }
        }

        return headings;
    }

    private static string? FirstText(params string?[] candidates)
    {
        return candidates.FirstOrDefault(text => !string.IsNullOrWhiteSpace(text))?.Trim();
    }
}
