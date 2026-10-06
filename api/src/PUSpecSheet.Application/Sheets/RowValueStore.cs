using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Reads and writes cell values, which live in one typed table per cell kind in the "values" schema.</summary>
public sealed class RowValueStore(PuSpecSheetDbContext db)
{
    /// <summary>Every value of the given row revisions, by revision id and then sheet cell id.</summary>
    public async Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadAsync(
        IReadOnlyCollection<int> revisionIds,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, Dictionary<int, CellValueBag>>();
        if (revisionIds.Count == 0)
        {
            return result;
        }

        foreach (var value in await db.TextValues.AsNoTracking().Where(v => revisionIds.Contains(v.SheetRowRevisionId)).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Text = value.Value;
        }

        foreach (var value in await db.NumericValues.AsNoTracking().Where(v => revisionIds.Contains(v.SheetRowRevisionId)).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Number = value.Value;
        }

        foreach (var value in await db.DateValues.AsNoTracking().Where(v => revisionIds.Contains(v.SheetRowRevisionId)).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Date = value.Value;
        }

        foreach (var value in await db.BooleanValues.AsNoTracking().Where(v => revisionIds.Contains(v.SheetRowRevisionId)).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Boolean = value.Value;
        }

        foreach (var value in await db.OptionValues.AsNoTracking().Where(v => revisionIds.Contains(v.SheetRowRevisionId)).ToListAsync(cancellationToken))
        {
            BagFor(result, value).OptionId = value.CellTypeOptionId;
        }

        return result;
    }

    /// <summary>Copies every value of one row revision onto another, so a new draft starts from what is published.</summary>
    public async Task CopyAsync(int fromRevisionId, int toRevisionId, CancellationToken cancellationToken)
    {
        foreach (var value in await db.TextValues.AsNoTracking().Where(v => v.SheetRowRevisionId == fromRevisionId).ToListAsync(cancellationToken))
        {
            db.TextValues.Add(new TextValue { SheetRowRevisionId = toRevisionId, SheetCellId = value.SheetCellId, Value = value.Value });
        }

        foreach (var value in await db.NumericValues.AsNoTracking().Where(v => v.SheetRowRevisionId == fromRevisionId).ToListAsync(cancellationToken))
        {
            db.NumericValues.Add(new NumericValue { SheetRowRevisionId = toRevisionId, SheetCellId = value.SheetCellId, Value = value.Value });
        }

        foreach (var value in await db.DateValues.AsNoTracking().Where(v => v.SheetRowRevisionId == fromRevisionId).ToListAsync(cancellationToken))
        {
            db.DateValues.Add(new DateValue { SheetRowRevisionId = toRevisionId, SheetCellId = value.SheetCellId, Value = value.Value });
        }

        foreach (var value in await db.BooleanValues.AsNoTracking().Where(v => v.SheetRowRevisionId == fromRevisionId).ToListAsync(cancellationToken))
        {
            db.BooleanValues.Add(new BooleanValue { SheetRowRevisionId = toRevisionId, SheetCellId = value.SheetCellId, Value = value.Value });
        }

        foreach (var value in await db.OptionValues.AsNoTracking().Where(v => v.SheetRowRevisionId == fromRevisionId).ToListAsync(cancellationToken))
        {
            db.OptionValues.Add(new OptionValue { SheetRowRevisionId = toRevisionId, SheetCellId = value.SheetCellId, CellTypeOptionId = value.CellTypeOptionId });
        }
    }

    /// <summary>
    /// Sets, replaces or clears cell values in a draft row revision, reading what the revision already holds
    /// once per kind of value rather than once per cell. The values are already validated. When a cell is
    /// listed more than once, its last value is the one kept.
    /// </summary>
    public async Task SetManyAsync(int revisionId, IReadOnlyList<CellValueChange> changes, CancellationToken cancellationToken)
    {
        await ApplyAsync(
            db.TextValues,
            revisionId,
            Of(changes, CellKind.Text),
            request => !string.IsNullOrEmpty(request.Text),
            (value, request) => value.Value = request.Text!,
            cancellationToken);
        await ApplyAsync(
            db.NumericValues,
            revisionId,
            Of(changes, CellKind.Number),
            request => request.Number is not null,
            (value, request) => value.Value = request.Number!.Value,
            cancellationToken);
        await ApplyAsync(
            db.DateValues,
            revisionId,
            Of(changes, CellKind.Date),
            request => request.Date is not null,
            (value, request) => value.Value = request.Date!.Value,
            cancellationToken);
        await ApplyAsync(
            db.BooleanValues,
            revisionId,
            Of(changes, CellKind.Checkbox),
            request => request.Boolean is not null,
            (value, request) => value.Value = request.Boolean!.Value,
            cancellationToken);
        await ApplyAsync(
            db.OptionValues,
            revisionId,
            Of(changes, CellKind.TextDropdown, CellKind.NumberDropdown),
            request => request.OptionId is not null,
            (value, request) => value.CellTypeOptionId = request.OptionId!.Value,
            cancellationToken);
    }

    /// <summary>The changes for cells of the given kinds, one per cell: a cell's last change wins.</summary>
    private static List<CellValueChange> Of(IReadOnlyList<CellValueChange> changes, params CellKind[] kinds)
    {
        return changes
            .Where(change => kinds.Contains(change.Kind))
            .GroupBy(change => change.SheetCellId)
            .Select(cell => cell.Last())
            .ToList();
    }

    /// <summary>
    /// Applies the changes to one value table: a cell with nothing to hold loses its row, a cell that already
    /// has a row has it updated, and any other cell gets a new one.
    /// </summary>
    private static async Task ApplyAsync<TValue>(
        DbSet<TValue> set,
        int revisionId,
        List<CellValueChange> changes,
        Func<CellValueRequest, bool> hasValue,
        Action<TValue, CellValueRequest> assign,
        CancellationToken cancellationToken)
        where TValue : class, ICellValue, new()
    {
        if (changes.Count == 0)
        {
            return;
        }

        var cellIds = changes.Select(change => change.SheetCellId).ToList();
        var existing = await set
            .Where(value => value.SheetRowRevisionId == revisionId && cellIds.Contains(value.SheetCellId))
            .ToDictionaryAsync(value => value.SheetCellId, cancellationToken);
        foreach (var change in changes)
        {
            var value = existing.GetValueOrDefault(change.SheetCellId);
            if (!hasValue(change.Value))
            {
                if (value is not null)
                {
                    set.Remove(value);
                }

                continue;
            }

            if (value is null)
            {
                value = new TValue { SheetRowRevisionId = revisionId, SheetCellId = change.SheetCellId };
                set.Add(value);
            }

            assign(value, change.Value);
        }
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
