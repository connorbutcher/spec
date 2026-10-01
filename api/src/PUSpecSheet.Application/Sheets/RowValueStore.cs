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

    /// <summary>Sets, replaces or clears one cell's value in a draft row revision. The value is already validated.</summary>
    public async Task SetAsync(int revisionId, int cellId, CellKind kind, CellValueRequest request, CancellationToken cancellationToken)
    {
        switch (kind)
        {
            case CellKind.Text:
                {
                    var existing = await db.TextValues.SingleOrDefaultAsync(v => v.SheetRowRevisionId == revisionId && v.SheetCellId == cellId, cancellationToken);
                    if (string.IsNullOrEmpty(request.Text))
                    {
                        Remove(db.TextValues, existing);
                    }
                    else if (existing is null)
                    {
                        db.TextValues.Add(new TextValue { SheetRowRevisionId = revisionId, SheetCellId = cellId, Value = request.Text });
                    }
                    else
                    {
                        existing.Value = request.Text;
                    }

                    break;
                }

            case CellKind.Number:
                {
                    var existing = await db.NumericValues.SingleOrDefaultAsync(v => v.SheetRowRevisionId == revisionId && v.SheetCellId == cellId, cancellationToken);
                    if (request.Number is not { } number)
                    {
                        Remove(db.NumericValues, existing);
                    }
                    else if (existing is null)
                    {
                        db.NumericValues.Add(new NumericValue { SheetRowRevisionId = revisionId, SheetCellId = cellId, Value = number });
                    }
                    else
                    {
                        existing.Value = number;
                    }

                    break;
                }

            case CellKind.Date:
                {
                    var existing = await db.DateValues.SingleOrDefaultAsync(v => v.SheetRowRevisionId == revisionId && v.SheetCellId == cellId, cancellationToken);
                    if (request.Date is not { } date)
                    {
                        Remove(db.DateValues, existing);
                    }
                    else if (existing is null)
                    {
                        db.DateValues.Add(new DateValue { SheetRowRevisionId = revisionId, SheetCellId = cellId, Value = date });
                    }
                    else
                    {
                        existing.Value = date;
                    }

                    break;
                }

            case CellKind.Checkbox:
                {
                    var existing = await db.BooleanValues.SingleOrDefaultAsync(v => v.SheetRowRevisionId == revisionId && v.SheetCellId == cellId, cancellationToken);
                    if (request.Boolean is not { } flag)
                    {
                        Remove(db.BooleanValues, existing);
                    }
                    else if (existing is null)
                    {
                        db.BooleanValues.Add(new BooleanValue { SheetRowRevisionId = revisionId, SheetCellId = cellId, Value = flag });
                    }
                    else
                    {
                        existing.Value = flag;
                    }

                    break;
                }

            case CellKind.TextDropdown:
            case CellKind.NumberDropdown:
                {
                    var existing = await db.OptionValues.SingleOrDefaultAsync(v => v.SheetRowRevisionId == revisionId && v.SheetCellId == cellId, cancellationToken);
                    if (request.OptionId is not { } optionId)
                    {
                        Remove(db.OptionValues, existing);
                    }
                    else if (existing is null)
                    {
                        db.OptionValues.Add(new OptionValue { SheetRowRevisionId = revisionId, SheetCellId = cellId, CellTypeOptionId = optionId });
                    }
                    else
                    {
                        existing.CellTypeOptionId = optionId;
                    }

                    break;
                }
        }
    }

    private static void Remove<TValue>(DbSet<TValue> set, TValue? existing)
        where TValue : class
    {
        if (existing is not null)
        {
            set.Remove(existing);
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
