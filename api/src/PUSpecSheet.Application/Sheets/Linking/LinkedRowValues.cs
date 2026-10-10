using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Linking;

/// <summary>
/// Reads a row of a sheet table as a linked dropdown gives it: each value as text with a label, the row's
/// own cells first and then each set of repeated columns, left to right. A value in a set is labelled
/// with the set's name, which is the first value at the top of it in the table's header.
/// </summary>
internal sealed class LinkedRowValues
{
    private readonly SheetSnapshot snapshot;
    private readonly Dictionary<int, string> blockNames = [];
    private readonly Dictionary<int, int> blockOrder;

    public LinkedRowValues(SheetSnapshot snapshot)
    {
        this.snapshot = snapshot;
        blockOrder = snapshot.ColumnBlocks.ToDictionary(
            block => block.Id,
            block => snapshot.ColumnBlockRevisions.GetValueOrDefault(block.Id)?.Shown?.DisplayOrder ?? 0);

        var sections = snapshot.Sections.ToDictionary(section => section.Id);
        var headerCells = snapshot.Rows
            .Where(row => sections[row.SheetSectionId].TemplateSection.Role == SectionRole.Header)
            .OrderBy(row => snapshot.RowRevisions.GetValueOrDefault(row.Id)?.Shown?.DisplayOrder ?? 0)
            .ThenBy(row => row.Id)
            .SelectMany(row => row.Cells.OrderBy(cell => cell.TemplateCell.Column).Select(cell => (Row: row, Cell: cell)));
        foreach (var (row, cell) in headerCells)
        {
            if (cell.SheetColumnBlockId is { } blockId && !blockNames.ContainsKey(blockId) && TextOf(row, cell) is { } name)
            {
                blockNames[blockId] = name;
            }
        }
    }

    /// <summary>
    /// The row's values, each with the template cell it belongs to so a caller can leave one out.
    /// </summary>
    /// <param name="row">The row to read.</param>
    /// <param name="cells">The row's cells that are on the sheet.</param>
    /// <param name="labels">What each of the table's linkable columns is called, by template cell.</param>
    public List<(int SheetCellId, SheetLinkedValueDto Value)> Of(
        SheetRow row,
        IEnumerable<SheetCell> cells,
        IReadOnlyDictionary<int, string> labels)
    {
        var result = new List<(int SheetCellId, SheetLinkedValueDto Value)>();
        var ordered = cells
            .OrderBy(cell => cell.SheetColumnBlockId is { } blockId ? 1 + blockOrder.GetValueOrDefault(blockId) : 0)
            .ThenBy(cell => cell.SheetColumnBlockId ?? 0)
            .ThenBy(cell => cell.TemplateCell.Column)
            .ThenBy(cell => cell.Id);
        foreach (var cell in ordered)
        {
            if (!labels.TryGetValue(cell.TemplateCellId, out var label) || TextOf(row, cell) is not { } text)
            {
                continue;
            }

            // The value that names a set is not labelled with itself.
            var named = cell.SheetColumnBlockId is { } blockId
                && blockNames.TryGetValue(blockId, out var block)
                && !string.Equals(block, text, StringComparison.Ordinal)
                ? $"{block} {label}"
                : label;
            result.Add((cell.Id, new SheetLinkedValueDto(named, text)));
        }

        return result;
    }

    private string? TextOf(SheetRow row, SheetCell cell)
    {
        if (snapshot.RowRevisions.GetValueOrDefault(row.Id)?.Shown is not { } shown
            || !snapshot.CellTypes.TryGetValue(cell.TemplateCell.CellTypeId, out var cellType)
            || !cellType.Kind.CanBeLinkedTo())
        {
            return null;
        }

        return LinkedOptionText.Of(snapshot.Values.GetValueOrDefault(shown.Id)?.GetValueOrDefault(cell.Id), cellType);
    }
}
