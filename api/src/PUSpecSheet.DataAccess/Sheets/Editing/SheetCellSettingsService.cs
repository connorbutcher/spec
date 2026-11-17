using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Abstractions.CellTypes.LabelledText;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.CellTypes;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Drafts;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Reading;
using PUSpecSheet.DataAccess.Sheets.Values;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Editing;

public sealed class SheetCellSettingsService : ISheetCellSettingsService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly RowDrafts _drafts;
    private readonly RowDraftStarter _draftStarter;
    private readonly SheetReader _reader;

    public SheetCellSettingsService(
        PuSpecSheetDbContext db,
        RowDrafts drafts,
        RowDraftStarter draftStarter,
        SheetReader reader)
    {
        _db = db;
        _drafts = drafts;
        _draftStarter = draftStarter;
        _reader = reader;
    }

    public async Task<SheetDto> SaveAsync(int rowId, SaveRowCellSettingsRequest request, CancellationToken cancellationToken)
    {
        var row = await LoadRowAsync(rowId, cancellationToken);
        var sheetId = row.SheetSection.SheetTable.SheetId;

        // Check every setting before locking the row or changing anything, against the sheet as the user sees it.
        var sheet = await _reader.ReadLiveAsync(sheetId, cancellationToken);
        var cellsById = row.Cells.ToDictionary(cell => cell.Id);
        var wanted = WantedSettings(request, cellsById, sheet);

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var draft = await _draftStarter.StartAsync(rowId, cancellationToken);
        await SaveChangedAsync(draft.Id, wanted, cellsById, cancellationToken);
        await _db.SaveSheetChangesAsync(cancellationToken);

        // Settings put back to what is published leave nothing to publish, so the row is released.
        await _drafts.ReleaseIfUnchangedAsync(rowId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await _reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private static Dictionary<int, CellInstanceSettings?> WantedSettings(
        SaveRowCellSettingsRequest request,
        Dictionary<int, DbSheetCell> cellsById,
        SheetDto sheet)
    {
        var wanted = new Dictionary<int, CellInstanceSettings?>();
        foreach (var entry in request.Settings)
        {
            if (!cellsById.TryGetValue(entry.SheetCellId, out var cell))
            {
                throw new InvalidRequestException($"Cell {entry.SheetCellId} isn't in this row.");
            }

            CellInstanceSettingsValidator.Validate(NameOf(cell), cell.TemplateCell.CellType.Kind, entry.Settings, sheet);
            wanted[entry.SheetCellId] = entry.Settings is { IsEmpty: false } ? entry.Settings : null;
        }

        return wanted;
    }

    private static string NameOf(DbSheetCell cell)
    {
        var template = cell.TemplateCell;
        return string.IsNullOrWhiteSpace(template.Caption) ? template.CellType.Name : template.Caption;
    }

    private async Task<DbSheetRow> LoadRowAsync(int rowId, CancellationToken cancellationToken)
    {
        return await SheetRowQueries.FindWithCellTypesAsync(_db, rowId, withOptions: false, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
    }

    /// <summary>Saves the settings that differ from the draft's, and clears the values chosen under the old ones.</summary>
    private async Task SaveChangedAsync(
        int draftId,
        Dictionary<int, CellInstanceSettings?> wanted,
        Dictionary<int, DbSheetCell> cellsById,
        CancellationToken cancellationToken)
    {
        var held = await RowValueQueries.LoadAsync(_db, draftId, cancellationToken);
        var changed = wanted
            .Where(entry => held.GetValueOrDefault(entry.Key)?.Settings != entry.Value)
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        await RowValueQueries.SetSettingsAsync(_db, draftId, changed, cancellationToken);

        // A labelled text cell keeps the values of the slots it still has; any other cell starts again.
        foreach (var (cellId, settings) in changed)
        {
            var templateCell = cellsById[cellId].TemplateCell;
            if (templateCell.CellType.Kind == CellKind.LabelledText)
            {
                var slots = LabelledTextSlots.Effective(CellSettingsResolver.ResolveConfiguration(templateCell), settings);
                await RowValueQueries.ArrangeSlotsAsync(_db, draftId, cellId, slots, cancellationToken);
            }
        }

        var cleared = changed.Keys
            .Where(cellId => cellsById[cellId].TemplateCell.CellType.Kind != CellKind.LabelledText)
            .Select(cellId => new CellValueChange
            {
                SheetCellId = cellId,
                Kind = cellsById[cellId].TemplateCell.CellType.Kind,
                Value = new CellValueBag(),
            })
            .ToList();
        await RowValueQueries.SetManyAsync(_db, draftId, cleared, cancellationToken);
    }
}
