using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
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
        var row = await db.SheetRows
            .AsNoTracking()
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.SheetTable)
            .Include(candidate => candidate.Cells)
            .ThenInclude(cell => cell.TemplateCell)
            .ThenInclude(templateCell => templateCell.CellType)
            .AsSplitQuery()
            .SingleOrDefaultAsync(candidate => candidate.Id == rowId, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
        var sheetId = row.SheetSection.SheetTable.SheetId;

        // Check every setting before locking the row or changing anything, against the sheet as the user sees it.
        var sheet = await reader.ReadLiveAsync(sheetId, cancellationToken);
        var cellsById = row.Cells.ToDictionary(cell => cell.Id);
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

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var draft = await draftStarter.StartAsync(rowId, cancellationToken);

        var held = (await valueStore.LoadAsync([draft.Id], cancellationToken)).GetValueOrDefault(draft.Id) ?? [];
        var changed = wanted
            .Where(entry => held.GetValueOrDefault(entry.Key)?.Settings != entry.Value)
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        await valueStore.SetSettingsAsync(draft.Id, changed, cancellationToken);

        // A value was chosen under the old settings, so it goes with them.
        var cleared = changed.Keys
            .Select(cellId => new CellValueChange(
                cellId,
                cellsById[cellId].TemplateCell.CellType.Kind,
                new CellValueRequest(cellId, null, null, null, null, null)))
            .ToList();
        await valueStore.SetManyAsync(draft.Id, cleared, cancellationToken);
        await db.SaveSheetChangesAsync(cancellationToken);

        // Settings put back to what is published leave nothing to publish, so the row is released.
        await drafts.ReleaseIfUnchangedAsync(rowId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private static string NameOf(SheetCell cell)
    {
        var template = cell.TemplateCell;
        return string.IsNullOrWhiteSpace(template.Caption) ? template.CellType.Name : template.Caption;
    }
}
