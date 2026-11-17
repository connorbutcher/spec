using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Abstractions.CellTypes.LabelledText;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.DataAccess.Sheets.Values;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Values;

namespace PUSpecSheet.DataAccess.Sheets;

public sealed class CellValueDataService : ICellValueDataService
{
    private readonly PuSpecSheetDbContext _db;

    public CellValueDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadAsync(
        IReadOnlyCollection<int> revisionIds,
        CancellationToken cancellationToken)
    {
        return LoadAsync(RowRevisionScope.Of(revisionIds), cancellationToken);
    }

    public Task<Dictionary<int, CellValueBag>> LoadAsync(int revisionId, CancellationToken cancellationToken)
    {
        return LoadCellValuesAsync(revisionId, cancellationToken);
    }

    public Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadInViewAsync(int sheetId, DateTime? moment, CancellationToken cancellationToken)
    {
        var revisionIds = _db.SheetRowRevisions
            .Where(revision => revision.SheetId == sheetId)
            .InView(moment)
            .Select(revision => revision.Id);
        return LoadAsync(RowRevisionScope.Of(revisionIds), cancellationToken);
    }

    public Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadPublishedUpToAsync(int sheetId, DateTime? moment, CancellationToken cancellationToken)
    {
        var revisionIds = _db.SheetRowRevisions
            .Where(revision => revision.SheetId == sheetId)
            .PublishedUpTo(moment)
            .Select(revision => revision.Id);
        return LoadAsync(RowRevisionScope.Of(revisionIds), cancellationToken);
    }

    public async Task<List<ChosenSettings>> ListChosenSettingsAsync(IReadOnlyCollection<int> revisionIds, CancellationToken cancellationToken)
    {
        return await _db.CellSettings
            .AsNoTracking()
            .Where(value => revisionIds.Contains(value.SheetRowRevisionId))
            .Select(value => new ChosenSettings
            {
                SheetRowRevisionId = value.SheetRowRevisionId,
                Settings = value.Settings,
                TemplateCellId = value.SheetCell.TemplateCellId,
                SheetColumnBlockId = value.SheetCell.SheetColumnBlockId,
            })
            .ToListAsync(cancellationToken);
    }

    public void AddSettings(int revisionId, int cellId, CellInstanceSettings settings)
    {
        _db.CellSettings.Add(new DbCellSettingsValue { SheetRowRevisionId = revisionId, SheetCellId = cellId, Settings = settings });
    }

    private Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadAsync(
        RowRevisionScope revisions,
        CancellationToken cancellationToken)
    {
        return LoadCellValuesAsync(revisions, cancellationToken);
    }

    public async Task CopyAsync(int fromRevisionId, int toRevisionId, CancellationToken cancellationToken)
    {
        await _db.Database.ExecuteSqlRawAsync(RowValueCopySql.Build(_db.Model), [fromRevisionId, toRevisionId], cancellationToken);
    }

    public async Task SetManyAsync(int revisionId, IReadOnlyList<CellValueChange> changes, CancellationToken cancellationToken)
    {
        await ApplyAsync(
            _db.TextValues,
            revisionId,
            Of(changes, CellKind.Text, CellKind.LinkedDropdown),
            request => !string.IsNullOrEmpty(request.Text),
            (value, request) => value.Value = request.Text!,
            cancellationToken);
        await ApplyAsync(
            _db.NumericValues,
            revisionId,
            Of(changes, CellKind.Number),
            request => request.Number is not null,
            (value, request) => value.Value = request.Number!.Value,
            cancellationToken);
        await ApplyAsync(
            _db.DateValues,
            revisionId,
            Of(changes, CellKind.Date),
            request => request.Date is not null,
            (value, request) => value.Value = request.Date!.Value,
            cancellationToken);
        await ApplyAsync(
            _db.BooleanValues,
            revisionId,
            Of(changes, CellKind.Checkbox),
            request => request.Boolean is not null,
            (value, request) => value.Value = request.Boolean!.Value,
            cancellationToken);
        await ApplyAsync(
            _db.OptionValues,
            revisionId,
            Of(changes, CellKind.TextDropdown, CellKind.NumberDropdown),
            request => request.OptionId is not null,
            (value, request) => value.CellTypeOptionId = request.OptionId!.Value,
            cancellationToken);
        await SetSlotsAsync(revisionId, Of(changes, CellKind.LabelledText), cancellationToken);
    }

    public async Task ArrangeSlotsAsync(int revisionId, int cellId, IReadOnlyList<LabelledTextSlot> slots, CancellationToken cancellationToken)
    {
        var positions = slots.Select((slot, index) => (slot.Key, index)).ToDictionary(entry => entry.Key, entry => entry.index, StringComparer.Ordinal);
        var held = await _db.LabelledTextValues
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
                _db.LabelledTextValues.Remove(value);
            }
        }
    }

    private async Task SetSlotsAsync(int revisionId, List<CellValueChange> changes, CancellationToken cancellationToken)
    {
        if (changes.Count == 0)
        {
            return;
        }

        var cellIds = changes.Select(change => change.SheetCellId).ToList();
        var existing = await _db.LabelledTextValues
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
                    _db.LabelledTextValues.Remove(value);
                }
            }

            var position = 0;
            foreach (var (key, text) in wanted)
            {
                if (!held.TryGetValue(key, out var value))
                {
                    value = new DbLabelledTextValue { SheetRowRevisionId = revisionId, SheetCellId = change.SheetCellId, SlotKey = key, Value = text };
                    _db.LabelledTextValues.Add(value);
                }

                value.Value = text;
                value.Position = position++;
            }
        }
    }

    public async Task SetSettingsAsync(
        int revisionId,
        IReadOnlyDictionary<int, CellInstanceSettings?> settingsByCell,
        CancellationToken cancellationToken)
    {
        var cellIds = settingsByCell.Keys.ToList();
        var existing = await _db.CellSettings
            .Where(value => value.SheetRowRevisionId == revisionId && cellIds.Contains(value.SheetCellId))
            .ToDictionaryAsync(value => value.SheetCellId, cancellationToken);
        foreach (var (cellId, settings) in settingsByCell)
        {
            var value = existing.GetValueOrDefault(cellId);
            if (settings is null || settings.IsEmpty)
            {
                if (value is not null)
                {
                    _db.CellSettings.Remove(value);
                }

                continue;
            }

            if (value is null)
            {
                value = new DbCellSettingsValue { SheetRowRevisionId = revisionId, SheetCellId = cellId, Settings = settings };
                _db.CellSettings.Add(value);
            }

            value.Settings = settings;
        }
    }

    private static List<CellValueChange> Of(IReadOnlyList<CellValueChange> changes, params CellKind[] kinds)
    {
        return changes
            .Where(change => kinds.Contains(change.Kind))
            .GroupBy(change => change.SheetCellId)
            .Select(cell => cell.Last())
            .ToList();
    }

    private static async Task ApplyAsync<TValue>(
        DbSet<TValue> set,
        int revisionId,
        List<CellValueChange> changes,
        Func<CellValueBag, bool> hasValue,
        Action<TValue, CellValueBag> assign,
        CancellationToken cancellationToken)
        where TValue : class, IDbCellValue, new()
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

    public async Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadCellValuesAsync(
        RowRevisionScope revisions,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, Dictionary<int, CellValueBag>>();
        if (revisions.IsEmpty)
        {
            return result;
        }

        foreach (var value in await revisions.Apply(_db.TextValues.AsNoTracking()).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Text = value.Value;
        }

        foreach (var value in await revisions.Apply(_db.NumericValues.AsNoTracking()).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Number = value.Value;
        }

        foreach (var value in await revisions.Apply(_db.DateValues.AsNoTracking()).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Date = value.Value;
        }

        foreach (var value in await revisions.Apply(_db.BooleanValues.AsNoTracking()).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Boolean = value.Value;
        }

        foreach (var value in await revisions.Apply(_db.OptionValues.AsNoTracking()).ToListAsync(cancellationToken))
        {
            BagFor(result, value).OptionId = value.CellTypeOptionId;
        }

        foreach (var value in await revisions.Apply(_db.CellSettings.AsNoTracking()).ToListAsync(cancellationToken))
        {
            BagFor(result, value).Settings = value.Settings;
        }

        var slotValues = await revisions.Apply(_db.LabelledTextValues.AsNoTracking())
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

    public async Task<Dictionary<int, CellValueBag>> LoadCellValuesAsync(
        int rowRevisionId,
        CancellationToken cancellationToken)
    {
        var values = await _db.LoadCellValuesAsync(RowRevisionScope.Of([rowRevisionId]), cancellationToken);
        return values.GetValueOrDefault(rowRevisionId) ?? [];
    }

    private static CellValueBag BagFor(Dictionary<int, Dictionary<int, CellValueBag>> result, IDbCellValue value)
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
