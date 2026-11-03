using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Reads cell values from the typed tables of the "values" schema, by sheet cell id. Each query returns
/// only the cell id and the value, and finds them through the row revisions the cell query already found,
/// so no value query reads the revisions table again.
/// </summary>
public sealed class PublishedValueReader(PuSpecSheetDbContext db)
{
    /// <param name="cells">The cells to read values for, as the cell query found them.</param>
    /// <param name="wholeSheet">True when the cells are every cell of the sheet, so every value of their revisions is read and not a long list of cells.</param>
    /// <param name="cancellationToken">Stops the queries.</param>
    public async Task<Dictionary<int, object>> ReadAsync(
        IReadOnlyList<PublishedCellRecord> cells,
        bool wholeSheet,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, object>();
        if (cells.Count == 0)
        {
            return result;
        }

        var revisionIds = cells.Select(cell => cell.RowRevisionId).Distinct().ToList();
        var cellIds = wholeSheet ? null : cells.Select(cell => cell.CellId).ToList();

        var texts = await Scope(db.TextValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var text in texts)
        {
            result[text.SheetCellId] = text.Value;
        }

        var numbers = await Scope(db.NumericValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var number in numbers)
        {
            result[number.SheetCellId] = PublishedValues.Number(number.Value);
        }

        var dates = await Scope(db.DateValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var date in dates)
        {
            result[date.SheetCellId] = date.Value;
        }

        var booleans = await Scope(db.BooleanValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var boolean in booleans)
        {
            result[boolean.SheetCellId] = boolean.Value;
        }

        var options = await Scope(db.OptionValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.CellTypeOption.Value, value.CellTypeOption.CellType.Kind })
            .ToListAsync(cancellationToken);
        foreach (var option in options)
        {
            result[option.SheetCellId] = PublishedValues.Option(option.Value, option.Kind);
        }

        return result;
    }

    /// <summary>
    /// The values of one typed table held by the row revisions found, and only for the cells listed when
    /// there are any (null for a whole sheet).
    /// </summary>
    /// <remarks>
    /// A whole sheet's revisions are sent as one JSON parameter. As many separate parameters, a large sheet
    /// costs a fresh plan for every count of rows, and a join back to the revisions instead reads the
    /// revisions of every sheet in the database, because SQL Server can't tell how few belong to this one.
    /// </remarks>
    public static IQueryable<TValue> Scope<TValue>(
        IQueryable<TValue> values,
        List<int> revisionIds,
        List<int>? cellIds)
        where TValue : class, ICellValue
    {
        if (cellIds is not null)
        {
            return values
                .AsNoTracking()
                .Where(value => revisionIds.Contains(value.SheetRowRevisionId) && cellIds.Contains(value.SheetCellId));
        }

        return values
            .AsNoTracking()
            .Where(value => EF.Parameter(revisionIds).Contains(value.SheetRowRevisionId));
    }
}
