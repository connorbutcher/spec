using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Reads cell values from the typed tables of the "values" schema, by sheet cell id. Each query returns
/// only the cell id and the value. For a whole sheet the values are found through the row revisions at the
/// version; for a selection, through the cells and revisions the cell query already found.
/// </summary>
public sealed class PublishedValueReader(PuSpecSheetDbContext db)
{
    /// <param name="version">The version to read.</param>
    /// <param name="selected">The cells to read values for, or null for every value on the sheet.</param>
    /// <param name="cancellationToken">Stops the queries.</param>
    public async Task<Dictionary<int, object>> ReadAsync(
        ResolvedSheetVersion version,
        IReadOnlyList<PublishedCellRecord>? selected,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, object>();
        if (selected is { Count: 0 })
        {
            return result;
        }

        var revisionIds = selected?.Select(cell => cell.RowRevisionId).Distinct().ToList();
        var cellIds = selected?.Select(cell => cell.CellId).ToList();

        var texts = await Scope(db.TextValues, version, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var text in texts)
        {
            result[text.SheetCellId] = text.Value;
        }

        var numbers = await Scope(db.NumericValues, version, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var number in numbers)
        {
            result[number.SheetCellId] = PublishedValues.Number(number.Value);
        }

        var dates = await Scope(db.DateValues, version, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var date in dates)
        {
            result[date.SheetCellId] = date.Value;
        }

        var booleans = await Scope(db.BooleanValues, version, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.Value })
            .ToListAsync(cancellationToken);
        foreach (var boolean in booleans)
        {
            result[boolean.SheetCellId] = boolean.Value;
        }

        var options = await Scope(db.OptionValues, version, revisionIds, cellIds)
            .Select(value => new { value.SheetCellId, value.CellTypeOption.Value, value.CellTypeOption.CellType.Kind })
            .ToListAsync(cancellationToken);
        foreach (var option in options)
        {
            result[option.SheetCellId] = PublishedValues.Option(option.Value, option.Kind);
        }

        return result;
    }

    /// <summary>The values of one typed table to read: by the cells and revisions found, or for the whole sheet.</summary>
    public static IQueryable<TValue> Scope<TValue>(
        IQueryable<TValue> values,
        ResolvedSheetVersion version,
        List<int>? revisionIds,
        List<int>? cellIds)
        where TValue : class, ICellValue
    {
        if (revisionIds is not null && cellIds is not null)
        {
            return values
                .AsNoTracking()
                .Where(value => revisionIds.Contains(value.SheetRowRevisionId) && cellIds.Contains(value.SheetCellId));
        }

        var sheetId = version.SheetId;
        var moment = version.PublishedAtUtc;
        return values
            .AsNoTracking()
            .Where(value => value.SheetRowRevision.SheetRow.SheetSection.SheetTable.SheetId == sheetId
                && value.SheetRowRevision.Status == RevisionStatus.Published
                && value.SheetRowRevision.PublishedAtUtc <= moment
                && (value.SheetRowRevision.SupersededAtUtc == null || value.SheetRowRevision.SupersededAtUtc > moment)
                && !value.SheetRowRevision.IsDeleted);
    }
}
