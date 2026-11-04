using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

public sealed class SheetCellSettingsService(
    PuSpecSheetDbContext db,
    RowDrafts drafts,
    RowDraftStarter draftStarter,
    RowValueStore valueStore,
    SheetReader reader) : ISheetCellSettingsService
{
    public async Task<SheetDto> SaveAsync(int rowId, SaveRowCellSettingsRequest request, CancellationToken cancellationToken)
    {
        var row = await LoadRowAsync(rowId, cancellationToken);
        var sheetId = row.SheetSection.SheetTable.SheetId;

        // Check every setting before locking the row or changing anything, against the sheet as the user sees it.
        var sheet = await reader.ReadLiveAsync(sheetId, cancellationToken);
        var cellsById = row.Cells.ToDictionary(cell => cell.Id);
        var wanted = WantedSettings(request, cellsById, sheet);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var draft = await draftStarter.StartAsync(rowId, cancellationToken);
        await SaveChangedAsync(draft.Id, wanted, cellsById, cancellationToken);
        await db.SaveSheetChangesAsync(cancellationToken);

        // Settings put back to what is published leave nothing to publish, so the row is released.
        await drafts.ReleaseIfUnchangedAsync(rowId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private static Dictionary<int, CellInstanceSettings?> WantedSettings(
        SaveRowCellSettingsRequest request,
        Dictionary<int, SheetCell> cellsById,
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

    private static string NameOf(SheetCell cell)
    {
        var template = cell.TemplateCell;
        return string.IsNullOrWhiteSpace(template.Caption) ? template.CellType.Name : template.Caption;
    }

    private async Task<SheetRow> LoadRowAsync(int rowId, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .AsNoTracking()
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.SheetTable)
            .Include(candidate => candidate.Cells)
            .ThenInclude(cell => cell.TemplateCell)
            .ThenInclude(templateCell => templateCell.CellType)
            .AsSplitQuery()
            .SingleOrDefaultAsync(candidate => candidate.Id == rowId, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
    }

    /// <summary>Saves the settings that differ from the draft's, and clears the values chosen under the old ones.</summary>
    private async Task SaveChangedAsync(
        int draftId,
        Dictionary<int, CellInstanceSettings?> wanted,
        Dictionary<int, SheetCell> cellsById,
        CancellationToken cancellationToken)
    {
        var held = (await valueStore.LoadAsync([draftId], cancellationToken)).GetValueOrDefault(draftId) ?? [];
        var changed = wanted
            .Where(entry => held.GetValueOrDefault(entry.Key)?.Settings != entry.Value)
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        await valueStore.SetSettingsAsync(draftId, changed, cancellationToken);

        var cleared = changed.Keys
            .Select(cellId => new CellValueChange(
                cellId,
                cellsById[cellId].TemplateCell.CellType.Kind,
                new CellValueRequest(cellId, null, null, null, null, null)))
            .ToList();
        await valueStore.SetManyAsync(draftId, cleared, cancellationToken);
    }
}
