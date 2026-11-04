using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Reads the values of cells whose kinds are known, and only asks the value tables those kinds use: rows
/// of text and numbers cost two queries, not one for every kind of value there is.
/// </summary>
public sealed class PublishedKindValueReader(PuSpecSheetDbContext db)
{
    /// <param name="cells">The cells to read values for.</param>
    /// <param name="wholeSheet">True when the cells are every cell of the sheet, so every value of their revisions is read and not a long list of cells.</param>
    /// <param name="cancellationToken">Stops the queries.</param>
    public async Task<Dictionary<int, object>> ReadAsync(
        IReadOnlyList<PublishedLookupCellRecord> cells,
        bool wholeSheet,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, object>();
        var valueCells = cells.Where(cell => cell.Kind.StoresValue()).ToList();
        if (valueCells.Count == 0)
        {
            return result;
        }

        var revisionIds = valueCells.Select(cell => cell.RowRevisionId).Distinct().ToList();
        if (CellsOf(valueCells, wholeSheet, out var textIds, CellKind.Text, CellKind.LinkedDropdown))
        {
            await PublishedValueReader.ReadTextsAsync(db, result, revisionIds, textIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var numberIds, CellKind.Number))
        {
            await PublishedValueReader.ReadNumbersAsync(db, result, revisionIds, numberIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var dateIds, CellKind.Date))
        {
            await PublishedValueReader.ReadDatesAsync(db, result, revisionIds, dateIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var booleanIds, CellKind.Checkbox))
        {
            await PublishedValueReader.ReadBooleansAsync(db, result, revisionIds, booleanIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var optionIds, CellKind.TextDropdown, CellKind.NumberDropdown))
        {
            await PublishedValueReader.ReadOptionsAsync(db, result, revisionIds, optionIds, cancellationToken);
        }

        return result;
    }

    /// <summary>Whether any cell is of the kinds, and the ids of those cells (null for a whole sheet).</summary>
    private static bool CellsOf(List<PublishedLookupCellRecord> cells, bool wholeSheet, out List<int>? ids, params CellKind[] kinds)
    {
        var matching = cells.Where(cell => kinds.Contains(cell.Kind)).Select(cell => cell.CellId).ToList();
        ids = wholeSheet ? null : matching;
        return matching.Count > 0;
    }
}
