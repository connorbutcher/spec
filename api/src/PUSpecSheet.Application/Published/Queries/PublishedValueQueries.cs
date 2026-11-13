using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Published.Sheets;
using PUSpecSheet.Application.Published.Versions;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Values;

namespace PUSpecSheet.Application.Published.Queries;

/// <summary>
/// Reads cell values from the typed tables of the "values" schema, by sheet cell id. Each query returns
/// only the cell id and the value. A whole sheet's values are found through the sheet's revisions at the
/// version; selected cells through the row revisions the cell query found.
/// </summary>
public static class PublishedValueQueries
{
    /// <param name="db">The database.</param>
    /// <param name="cells">The cells to read values for, as the cell query found them.</param>
    /// <param name="wholeSheet">The version, when the cells are every cell of the sheet, so every value of its revisions is read and not a long list of cells; otherwise null.</param>
    /// <param name="cancellationToken">Stops the queries.</param>
    public static async Task<Dictionary<int, object>> ReadAsync(
        PuSpecSheetDbContext db,
        IReadOnlyList<PublishedCellRecord> cells,
        ResolvedSheetVersion? wholeSheet,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, object>();
        if (cells.Count == 0)
        {
            return result;
        }

        var revisionIds = RevisionsOf(db, cells.Select(cell => cell.RowRevisionId), wholeSheet);
        var cellIds = wholeSheet is not null ? null : cells.Select(cell => cell.CellId).ToList();

        await ReadTextsAsync(db, result, revisionIds, cellIds, cancellationToken);
        await ReadNumbersAsync(db, result, revisionIds, cellIds, cancellationToken);
        await ReadDatesAsync(db, result, revisionIds, cellIds, cancellationToken);
        await ReadBooleansAsync(db, result, revisionIds, cellIds, cancellationToken);
        await ReadOptionsAsync(db, result, revisionIds, cellIds, cancellationToken);
        await ReadLabelledTextsAsync(db, result, revisionIds, cellIds, cancellationToken);
        return result;
    }

    /// <summary>
    /// The row revisions to read values for: for a whole sheet, a query over the sheet's revisions at the
    /// version, which SQL Server runs inside each value query; otherwise the ids the cell query found.
    /// </summary>
    /// <remarks>
    /// A whole sheet's ids sent as one JSON parameter looked cheap, but SQL Server guesses a JSON list holds
    /// about 50 ids and, for a small value table, reads the whole list again for every value row: 63 of the
    /// 79 ms of a 2,000-row sheet went on 101 dates. The query is found through the (sheet, published) index.
    /// </remarks>
    public static RowRevisionScope RevisionsOf(PuSpecSheetDbContext db, IEnumerable<int> found, ResolvedSheetVersion? wholeSheet)
    {
        return wholeSheet is null
            ? RowRevisionScope.Of(found.Distinct().ToList())
            : RowRevisionScope.Of(db.RowRevisionIdsAt(wholeSheet));
    }

    /// <summary>
    /// The values of one typed table held by the row revisions in scope, and only for the cells listed when
    /// there are any (null for a whole sheet).
    /// </summary>
    public static IQueryable<TValue> Scope<TValue>(
        IQueryable<TValue> values,
        RowRevisionScope revisions,
        List<int>? cellIds)
        where TValue : class, IDbCellValue
    {
        var scoped = revisions.Apply(values.AsNoTracking());
        return cellIds is null ? scoped : scoped.Where(value => cellIds.Contains(value.SheetCellId));
    }

    public static async Task ReadTextsAsync(PuSpecSheetDbContext db, Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken)
    {
        var texts = await Scope(db.TextValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var text in texts)
        {
            result[text.SheetCellId] = text.Value;
        }
    }

    public static async Task ReadNumbersAsync(PuSpecSheetDbContext db, Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken)
    {
        var numbers = await Scope(db.NumericValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var number in numbers)
        {
            result[number.SheetCellId] = PublishedValues.Number(number.Value);
        }
    }

    public static async Task ReadDatesAsync(PuSpecSheetDbContext db, Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken)
    {
        var dates = await Scope(db.DateValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var date in dates)
        {
            result[date.SheetCellId] = date.Value;
        }
    }

    public static async Task ReadBooleansAsync(PuSpecSheetDbContext db, Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken)
    {
        var booleans = await Scope(db.BooleanValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var boolean in booleans)
        {
            result[boolean.SheetCellId] = boolean.Value;
        }
    }

    public static async Task ReadOptionsAsync(PuSpecSheetDbContext db, Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken)
    {
        var options = await Scope(db.OptionValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.CellTypeOption.Value, value.CellTypeOption.CellType.Kind })
            .ToListAsync(cancellationToken);
        foreach (var option in options)
        {
            result[option.SheetCellId] = PublishedValues.Option(option.Value, option.Kind);
        }
    }

    /// <summary>A labelled text cell's values, as an object of slot key to text in the cell's order, such as { "rh": "P-1003-R", "lh": "P-1003-L" }.</summary>
    public static async Task ReadLabelledTextsAsync(PuSpecSheetDbContext db, Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken)
    {
        var values = await Scope(db.LabelledTextValues, revisionIds, cellIds)
            .OrderBy(value => value.Position)
            .Select(value => new { value.SheetCellId, value.SlotKey, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var cell in values.GroupBy(value => value.SheetCellId))
        {
            result[cell.Key] = cell.ToDictionary(value => value.SlotKey, value => value.Value, StringComparer.Ordinal);
        }
    }
}
