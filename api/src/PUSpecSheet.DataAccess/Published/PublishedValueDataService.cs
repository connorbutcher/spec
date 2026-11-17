using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.DataAccess.Published.Lookups;
using PUSpecSheet.DataAccess.Published.Queries;
using PUSpecSheet.DataAccess.Published.Sheets;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Values;

namespace PUSpecSheet.DataAccess.Published;

public sealed class PublishedValueDataService : IPublishedValueDataService
{
    private readonly PuSpecSheetDbContext _db;

    public PublishedValueDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<Dictionary<int, object>> ReadAsync(
        IReadOnlyList<PublishedCellRecord> cells,
        ResolvedSheetVersion? wholeSheet,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, object>();
        if (cells.Count == 0)
        {
            return result;
        }

        var revisionIds = RevisionsOf(cells.Select(cell => cell.RowRevisionId), wholeSheet);
        var cellIds = wholeSheet is not null ? null : cells.Select(cell => cell.CellId).ToList();

        await ReadTextsAsync(result, revisionIds, cellIds, cancellationToken);
        await ReadNumbersAsync(result, revisionIds, cellIds, cancellationToken);
        await ReadDatesAsync(result, revisionIds, cellIds, cancellationToken);
        await ReadBooleansAsync(result, revisionIds, cellIds, cancellationToken);
        await ReadOptionsAsync(result, revisionIds, cellIds, cancellationToken);
        await ReadLabelledTextsAsync(result, revisionIds, cellIds, cancellationToken);
        return result;
    }

    // A whole sheet's ids are read through a subquery, not sent as a JSON list: SQL Server guesses a JSON list
    // holds about 50 ids and rereads it for every value row, which cost 63 of 79 ms on a 2,000-row sheet.
    public RowRevisionScope RevisionsOf(IEnumerable<int> found, ResolvedSheetVersion? wholeSheet)
    {
        return wholeSheet is null
            ? RowRevisionScope.Of(found.Distinct().ToList())
            : RowRevisionScope.Of(_db.RowRevisionIdsAt(wholeSheet));
    }

    public IQueryable<TValue> Scope<TValue>(
        IQueryable<TValue> values,
        RowRevisionScope revisions,
        List<int>? cellIds)
        where TValue : class, IDbCellValue
    {
        var scoped = revisions.Apply(values.AsNoTracking());
        return cellIds is null ? scoped : scoped.Where(value => cellIds.Contains(value.SheetCellId));
    }

    public async Task ReadTextsAsync(Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken)
    {
        var texts = await Scope(_db.TextValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var text in texts)
        {
            result[text.SheetCellId] = text.Value;
        }
    }

    public async Task ReadNumbersAsync(Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken)
    {
        var numbers = await Scope(_db.NumericValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var number in numbers)
        {
            result[number.SheetCellId] = PublishedValues.Number(number.Value);
        }
    }

    public async Task ReadDatesAsync(Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken)
    {
        var dates = await Scope(_db.DateValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var date in dates)
        {
            result[date.SheetCellId] = date.Value;
        }
    }

    public async Task ReadBooleansAsync(Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken)
    {
        var booleans = await Scope(_db.BooleanValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var boolean in booleans)
        {
            result[boolean.SheetCellId] = boolean.Value;
        }
    }

    public async Task ReadOptionsAsync(Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken)
    {
        var options = await Scope(_db.OptionValues, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.CellTypeOption.Value, value.CellTypeOption.CellType.Kind })
            .ToListAsync(cancellationToken);
        foreach (var option in options)
        {
            result[option.SheetCellId] = PublishedValues.Option(option.Value, option.Kind);
        }
    }

    public async Task ReadLabelledTextsAsync(Dictionary<int, object> result, RowRevisionScope revisionIds, List<int>? cellIds, CancellationToken cancellationToken)
    {
        var values = await Scope(_db.LabelledTextValues, revisionIds, cellIds)
            .OrderBy(value => value.Position)
            .Select(value => new { value.SheetCellId, value.SlotKey, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var cell in values.GroupBy(value => value.SheetCellId))
        {
            result[cell.Key] = cell.ToDictionary(value => value.SlotKey, value => value.Value, StringComparer.Ordinal);
        }
    }

    public async Task<Dictionary<int, object>> ReadAsync(
        IReadOnlyList<PublishedLookupCellRecord> cells,
        ResolvedSheetVersion? wholeSheet,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, object>();
        var valueCells = cells.Where(cell => cell.Kind.StoresValue()).ToList();
        if (valueCells.Count == 0)
        {
            return result;
        }

        var revisionIds = RevisionsOf(valueCells.Select(cell => cell.RowRevisionId), wholeSheet);
        if (CellsOf(valueCells, wholeSheet, out var textIds, CellKind.Text, CellKind.LinkedDropdown))
        {
            await ReadTextsAsync(result, revisionIds, textIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var numberIds, CellKind.Number))
        {
            await ReadNumbersAsync(result, revisionIds, numberIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var dateIds, CellKind.Date))
        {
            await ReadDatesAsync(result, revisionIds, dateIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var booleanIds, CellKind.Checkbox))
        {
            await ReadBooleansAsync(result, revisionIds, booleanIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var optionIds, CellKind.TextDropdown, CellKind.NumberDropdown))
        {
            await ReadOptionsAsync(result, revisionIds, optionIds, cancellationToken);
        }

        if (CellsOf(valueCells, wholeSheet, out var labelledIds, CellKind.LabelledText))
        {
            await ReadLabelledTextsAsync(result, revisionIds, labelledIds, cancellationToken);
        }

        return result;
    }

    private static bool CellsOf(List<PublishedLookupCellRecord> cells, ResolvedSheetVersion? wholeSheet, out List<int>? ids, params CellKind[] kinds)
    {
        var matching = cells.Where(cell => kinds.Contains(cell.Kind)).Select(cell => cell.CellId).ToList();
        ids = wholeSheet is not null ? null : matching;
        return matching.Count > 0;
    }
}
