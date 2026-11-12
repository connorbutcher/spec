using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Data.Values;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;
using PUSpecSheet.Domain.CellTypes.LabelledText;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Application.Sheets.Values;

/// <summary>Reads and writes cell values, which live in one typed table per cell kind in the "values" schema, and the
/// settings chosen for cells on the sheet, which live beside them.</summary>
public sealed class RowValueStore(PuSpecSheetDbContext db)
{
    /// <summary>Every value of the given row revisions, by revision id and then sheet cell id.</summary>
    public Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadAsync(
        IReadOnlyCollection<int> revisionIds,
        CancellationToken cancellationToken)
    {
        return LoadAsync(RowRevisionScope.Of(revisionIds), cancellationToken);
    }

    /// <summary>Every value of the row revisions in scope, by revision id and then sheet cell id (see <see cref="CellValueQueries"/>).</summary>
    public Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadAsync(
        RowRevisionScope revisions,
        CancellationToken cancellationToken)
    {
        return db.LoadCellValuesAsync(revisions, cancellationToken);
    }

    /// <summary>Copies every value of one row revision onto another, so a new draft starts from what is published.</summary>
    /// <remarks>
    /// The copy happens in the database as one batch (see <see cref="RowValueCopySql"/>) and needs no save
    /// afterwards. The target revision must already be saved.
    /// </remarks>
    public async Task CopyAsync(int fromRevisionId, int toRevisionId, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(RowValueCopySql.Build(db.Model), [fromRevisionId, toRevisionId], cancellationToken);
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
            Of(changes, CellKind.Text, CellKind.LinkedDropdown),
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
        await SetSlotsAsync(revisionId, Of(changes, CellKind.LabelledText), cancellationToken);
    }

    /// <summary>
    /// Puts a draft's labelled text values in the order of the cell's slots and drops the values of slots it
    /// no longer has, after its slots changed on the sheet.
    /// </summary>
    public async Task ArrangeSlotsAsync(int revisionId, int cellId, IReadOnlyList<LabelledTextSlot> slots, CancellationToken cancellationToken)
    {
        var positions = slots.Select((slot, index) => (slot.Key, index)).ToDictionary(entry => entry.Key, entry => entry.index, StringComparer.Ordinal);
        var held = await db.LabelledTextValues
            .Where(value => value.SheetRowRevisionId == revisionId && value.SheetCellId == cellId)
            .ToListAsync(cancellationToken);
        foreach (var value in held)
        {
            if (positions.TryGetValue(value.SlotKey, out var position))
            {
                value.Position = position;
            }
            else
            {
                db.LabelledTextValues.Remove(value);
            }
        }
    }

    /// <summary>Replaces every value of each labelled text cell with the ones sent, in the order sent.</summary>
    private async Task SetSlotsAsync(int revisionId, List<CellValueChange> changes, CancellationToken cancellationToken)
    {
        if (changes.Count == 0)
        {
            return;
        }

        var cellIds = changes.Select(change => change.SheetCellId).ToList();
        var existing = await db.LabelledTextValues
            .Where(value => value.SheetRowRevisionId == revisionId && cellIds.Contains(value.SheetCellId))
            .ToListAsync(cancellationToken);
        var byCell = existing.ToLookup(value => value.SheetCellId);
        foreach (var change in changes)
        {
            var held = byCell[change.SheetCellId].ToDictionary(value => value.SlotKey, StringComparer.Ordinal);
            var wanted = SlotValues.Clean(change.Value.Slots) ?? [];
            foreach (var (key, value) in held)
            {
                if (!wanted.ContainsKey(key))
                {
                    db.LabelledTextValues.Remove(value);
                }
            }

            var position = 0;
            foreach (var (key, text) in wanted)
            {
                if (!held.TryGetValue(key, out var value))
                {
                    value = new LabelledTextValue { SheetRowRevisionId = revisionId, SheetCellId = change.SheetCellId, SlotKey = key };
                    db.LabelledTextValues.Add(value);
                }

                value.Value = text;
                value.Position = position++;
            }
        }
    }

    /// <summary>
    /// Sets, replaces or clears the settings chosen on the sheet for cells of a draft row revision. Settings
    /// with nothing set are stored as no record at all, so "nothing chosen" always compares the same.
    /// </summary>
    public async Task SetSettingsAsync(
        int revisionId,
        IReadOnlyDictionary<int, CellInstanceSettings?> settingsByCell,
        CancellationToken cancellationToken)
    {
        var cellIds = settingsByCell.Keys.ToList();
        var existing = await db.CellSettings
            .Where(value => value.SheetRowRevisionId == revisionId && cellIds.Contains(value.SheetCellId))
            .ToDictionaryAsync(value => value.SheetCellId, cancellationToken);
        foreach (var (cellId, settings) in settingsByCell)
        {
            var value = existing.GetValueOrDefault(cellId);
            if (settings is null || settings.IsEmpty)
            {
                if (value is not null)
                {
                    db.CellSettings.Remove(value);
                }

                continue;
            }

            if (value is null)
            {
                value = new CellSettingsValue { SheetRowRevisionId = revisionId, SheetCellId = cellId };
                db.CellSettings.Add(value);
            }

            value.Settings = settings;
        }
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
        Func<CellValueBag, bool> hasValue,
        Action<TValue, CellValueBag> assign,
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
}
