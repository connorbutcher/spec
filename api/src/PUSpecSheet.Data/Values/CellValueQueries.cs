using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Data.Values;

/// <summary>
/// The one place a cell is joined to the value its row revision holds. Values live in one typed table per
/// cell kind in the "values" schema, keyed by row revision and sheet cell, because the row is what is
/// versioned and locked; everything above this reads them as one <see cref="CellValueBag"/> per cell.
/// </summary>
public static class CellValueQueries
{
    /// <summary>Every value of the row revisions in scope, by revision id and then sheet cell id.</summary>
    public static async Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadCellValuesAsync(
        this PuSpecSheetDbContext db,
        RowRevisionScope revisions,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, Dictionary<int, CellValueBag>>();
        if (revisions.IsEmpty)
        {
            return result;
        }

        foreach (var value in await revisions.Apply(db.TextValues.AsNoTracking()).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Text = value.Value;
        }

        foreach (var value in await revisions.Apply(db.NumericValues.AsNoTracking()).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Number = value.Value;
        }

        foreach (var value in await revisions.Apply(db.DateValues.AsNoTracking()).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Date = value.Value;
        }

        foreach (var value in await revisions.Apply(db.BooleanValues.AsNoTracking()).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Boolean = value.Value;
        }

        foreach (var value in await revisions.Apply(db.OptionValues.AsNoTracking()).ToListAsync(cancellationToken))
        {
            BagFor(result, value).OptionId = value.CellTypeOptionId;
        }

        foreach (var value in await revisions.Apply(db.CellSettings.AsNoTracking()).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Settings = value.Settings;
        }

        var slotValues = await revisions.Apply(db.LabelledTextValues.AsNoTracking())
            .OrderBy(value => value.Position)
            .ToListAsync(cancellationToken);
        foreach (var value in slotValues)
        {
            var bag = BagFor(result, value);
            bag.Slots ??= new Dictionary<string, string>(StringComparer.Ordinal);
            bag.Slots[value.SlotKey] = value.Value;
        }

        return result;
    }

    /// <summary>
    /// The values of the cells as one revision of their row holds them, by sheet cell id: what callers want
    /// when they already know which revision of the row is shown.
    /// </summary>
    public static async Task<Dictionary<int, CellValueBag>> LoadCellValuesAsync(
        this PuSpecSheetDbContext db,
        int rowRevisionId,
        CancellationToken cancellationToken)
    {
        var values = await db.LoadCellValuesAsync(RowRevisionScope.Of([rowRevisionId]), cancellationToken);
        return values.GetValueOrDefault(rowRevisionId) ?? [];
    }

    private static CellValueBag BagFor(Dictionary<int, Dictionary<int, CellValueBag>> result, ICellValue value)
    {
        if (!result.TryGetValue(value.SheetRowRevisionId, out var cells))
        {
            cells = [];
            result[value.SheetRowRevisionId] = cells;
        }

        if (!cells.TryGetValue(value.SheetCellId, out var bag))
        {
            bag = new CellValueBag();
            cells[value.SheetCellId] = bag;
        }

        return bag;
    }
}
