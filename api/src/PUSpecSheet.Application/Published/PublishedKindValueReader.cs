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
    /// <param name="version">The version to read.</param>
    /// <param name="cells">The cells to read values for.</param>
    /// <param name="wholeSheet">True when the cells are every cell of the sheet, so the values are found through the sheet and not a long list of cells.</param>
    /// <param name="cancellationToken">Stops the queries.</param>
    public async Task<Dictionary<int, object>> ReadAsync(
        ResolvedSheetVersion version,
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

        var revisionIds = wholeSheet ? null : valueCells.Select(cell => cell.RowRevisionId).Distinct().ToList();

        if (CellsOf(valueCells, wholeSheet, out var textIds, CellKind.Text, CellKind.LinkedDropdown))
        {
            var texts = await PublishedValueReader.Scope(db.TextValues, version, revisionIds, textIds)
                .Select(value => new { value.SheetCellId, value.Value })
                .ToListAsync(cancellationToken);
            foreach (var text in texts)
            {
                result[text.SheetCellId] = text.Value;
            }
        }

        if (CellsOf(valueCells, wholeSheet, out var numberIds, CellKind.Number))
        {
            var numbers = await PublishedValueReader.Scope(db.NumericValues, version, revisionIds, numberIds)
                .Select(value => new { value.SheetCellId, value.Value })
                .ToListAsync(cancellationToken);
            foreach (var number in numbers)
            {
                result[number.SheetCellId] = PublishedValues.Number(number.Value);
            }
        }

        if (CellsOf(valueCells, wholeSheet, out var dateIds, CellKind.Date))
        {
            var dates = await PublishedValueReader.Scope(db.DateValues, version, revisionIds, dateIds)
                .Select(value => new { value.SheetCellId, value.Value })
                .ToListAsync(cancellationToken);
            foreach (var date in dates)
            {
                result[date.SheetCellId] = date.Value;
            }
        }

        if (CellsOf(valueCells, wholeSheet, out var booleanIds, CellKind.Checkbox))
        {
            var booleans = await PublishedValueReader.Scope(db.BooleanValues, version, revisionIds, booleanIds)
                .Select(value => new { value.SheetCellId, value.Value })
                .ToListAsync(cancellationToken);
            foreach (var boolean in booleans)
            {
                result[boolean.SheetCellId] = boolean.Value;
            }
        }

        if (CellsOf(valueCells, wholeSheet, out var optionIds, CellKind.TextDropdown, CellKind.NumberDropdown))
        {
            var options = await PublishedValueReader.Scope(db.OptionValues, version, revisionIds, optionIds)
                .Select(value => new { value.SheetCellId, value.CellTypeOption.Value, value.CellTypeOption.CellType.Kind })
                .ToListAsync(cancellationToken);
            foreach (var option in options)
            {
                result[option.SheetCellId] = PublishedValues.Option(option.Value, option.Kind);
            }
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
