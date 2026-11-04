using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Gives newly added rows the settings chosen for the row of the same kind that was added to the table
/// before them, so a column of linked dropdowns is pointed at its table once and not again for every row.
/// Only settings are carried over, never values.
/// </summary>
public sealed class NewRowSettings(PuSpecSheetDbContext db, ICurrentUser currentUser)
{
    /// <param name="tableId">The table the rows were added to.</param>
    /// <param name="newRowIds">The rows just added, which are still their author's first drafts.</param>
    /// <param name="cancellationToken">Stops the queries.</param>
    public async Task InheritAsync(int tableId, IReadOnlyCollection<int> newRowIds, CancellationToken cancellationToken)
    {
        if (newRowIds.Count == 0)
        {
            return;
        }

        var cells = await LoadCellsWithSettingsAsync(newRowIds, cancellationToken);
        if (cells.Count == 0)
        {
            return;
        }

        var templateRowByRow = await LoadTemplateRowsAsync(tableId, cells, cancellationToken);
        var shown = await ResolveShownAsync(templateRowByRow.Keys.ToList(), cancellationToken);

        // For each kind of row, the one added most recently before these.
        var earlierByTemplateRow = shown
            .Where(entry => !newRowIds.Contains(entry.Key) && RevisionResolver.IsVisible(entry.Value))
            .GroupBy(entry => templateRowByRow[entry.Key])
            .ToDictionary(group => group.Key, group => group.MaxBy(entry => entry.Key).Value.Shown!.Id);
        if (earlierByTemplateRow.Count == 0)
        {
            return;
        }

        var earlierRevisionIds = earlierByTemplateRow.Values.ToList();
        var chosen = await db.CellSettings
            .AsNoTracking()
            .Where(value => earlierRevisionIds.Contains(value.SheetRowRevisionId))
            .Select(value => new ChosenSettings(value.SheetRowRevisionId, value.Settings, value.SheetCell.TemplateCellId, value.SheetCell.SheetColumnBlockId))
            .ToListAsync(cancellationToken);

        AddInheritedSettings(cells, shown, templateRowByRow, earlierByTemplateRow, chosen);
        await db.SaveSheetChangesAsync(cancellationToken);
    }

    /// <summary>The same for the rows of a newly added section and of the sub-sections it starts with.</summary>
    public Task InheritAsync(SheetSection newSection, CancellationToken cancellationToken)
    {
        return InheritAsync(newSection.SheetTableId, RowIdsOf(newSection).ToList(), cancellationToken);
    }

    private static IEnumerable<int> RowIdsOf(SheetSection section)
    {
        return section.Rows
            .Select(row => row.Id)
            .Concat(section.ChildSheetSections.SelectMany(RowIdsOf));
    }

    private async Task<List<NewCell>> LoadCellsWithSettingsAsync(IReadOnlyCollection<int> newRowIds, CancellationToken cancellationToken)
    {
        var cells = await db.SheetCells
            .AsNoTracking()
            .Where(cell => newRowIds.Contains(cell.SheetRowId))
            .Select(cell => new NewCell(cell.Id, cell.SheetRowId, cell.TemplateCellId, cell.SheetColumnBlockId, cell.TemplateCell.CellType.Kind))
            .ToListAsync(cancellationToken);
        return cells.Where(cell => cell.Kind.HasInstanceSettings()).ToList();
    }

    /// <summary>Every row of the table built from the same template rows as the new cells' rows, with that template row.</summary>
    private async Task<Dictionary<int, int>> LoadTemplateRowsAsync(int tableId, List<NewCell> cells, CancellationToken cancellationToken)
    {
        var newRows = cells.Select(cell => cell.SheetRowId).ToHashSet();
        var templateRowIds = await db.SheetRows
            .Where(row => newRows.Contains(row.Id))
            .Select(row => row.TemplateRowId)
            .Distinct()
            .ToListAsync(cancellationToken);
        return await db.SheetRows
            .Where(row => row.SheetSection.SheetTableId == tableId && templateRowIds.Contains(row.TemplateRowId))
            .ToDictionaryAsync(row => row.Id, row => row.TemplateRowId, cancellationToken);
    }

    private async Task<Dictionary<int, RevisionResolution<SheetRowRevision>>> ResolveShownAsync(List<int> rowIds, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var revisions = await db.SheetRowRevisions
            .Where(revision => rowIds.Contains(revision.SheetRowId))
            .VisibleTo(me)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return RevisionResolver.Resolve(revisions, revision => revision.SheetRowId, me);
    }

    private void AddInheritedSettings(
        List<NewCell> cells,
        Dictionary<int, RevisionResolution<SheetRowRevision>> shown,
        Dictionary<int, int> templateRowByRow,
        Dictionary<int, int> earlierByTemplateRow,
        List<ChosenSettings> chosen)
    {
        foreach (var cell in cells)
        {
            if (shown.GetValueOrDefault(cell.SheetRowId)?.Shown is not { } mine
                || !earlierByTemplateRow.TryGetValue(templateRowByRow[cell.SheetRowId], out var earlierRevisionId))
            {
                continue;
            }

            var match = chosen.FirstOrDefault(candidate => candidate.SheetRowRevisionId == earlierRevisionId
                && candidate.TemplateCellId == cell.TemplateCellId
                && candidate.SheetColumnBlockId == cell.SheetColumnBlockId);
            if (match is not null)
            {
                db.CellSettings.Add(new CellSettingsValue { SheetRowRevisionId = mine.Id, SheetCellId = cell.Id, Settings = match.Settings });
            }
        }
    }
}
