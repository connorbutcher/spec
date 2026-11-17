using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Values;

// Gives newly added rows the settings chosen for the row of the same kind that was added to the table before them, so
// a column of linked dropdowns is pointed at its table once and not again for every row. Only settings are carried
// over, never values.
public sealed class NewRowSettings
{
    private readonly PuSpecSheetDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ISheetRowDataService _sheetRowData;
    private readonly ISheetCellDataService _sheetCellData;
    private readonly ICellValueDataService _cellValueData;

    public NewRowSettings(
        PuSpecSheetDbContext db,
        ICurrentUser currentUser,
        ISheetRowDataService sheetRowData,
        ISheetCellDataService sheetCellData,
        ICellValueDataService cellValueData)
    {
        _db = db;
        _currentUser = currentUser;
        _sheetRowData = sheetRowData;
        _sheetCellData = sheetCellData;
        _cellValueData = cellValueData;
    }

    public async Task InheritAsync(int tableId, IReadOnlyCollection<int> newRowIds, CancellationToken cancellationToken)
    {
        if (newRowIds.Count == 0)
        {
            return;
        }

        var newCells = await LoadCellsWithSettingsAsync(newRowIds, cancellationToken);
        if (newCells.Count == 0)
        {
            return;
        }

        var templateRowByRow = await LoadTemplateRowsAsync(tableId, newCells, cancellationToken);
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
        var chosen = await _cellValueData.ListChosenSettingsAsync(earlierRevisionIds, cancellationToken);

        AddInheritedSettings(newCells, shown, templateRowByRow, earlierByTemplateRow, chosen);
        await _db.SaveSheetChangesAsync(cancellationToken);
    }

    public Task InheritAsync(DbSheetSection newSection, CancellationToken cancellationToken)
    {
        return InheritAsync(newSection.SheetTableId, RowIdsOf(newSection).ToList(), cancellationToken);
    }

    private static IEnumerable<int> RowIdsOf(DbSheetSection section)
    {
        return section.Rows
            .Select(row => row.Id)
            .Concat(section.ChildSheetSections.SelectMany(RowIdsOf));
    }

    private async Task<List<NewCell>> LoadCellsWithSettingsAsync(IReadOnlyCollection<int> newRowIds, CancellationToken cancellationToken)
    {
        var rowCells = await _sheetCellData.ListNewCellsAsync(newRowIds, cancellationToken);
        return rowCells.Where(cell => cell.Kind.HasInstanceSettings()).ToList();
    }

    private async Task<Dictionary<int, int>> LoadTemplateRowsAsync(int tableId, List<NewCell> newCells, CancellationToken cancellationToken)
    {
        var newRows = newCells.Select(cell => cell.SheetRowId).ToHashSet();
        var templateRowIds = await _sheetRowData.ListTemplateRowIdsAsync(newRows, cancellationToken);
        return await _sheetRowData.GetTemplateRowsOnTableAsync(tableId, templateRowIds, cancellationToken);
    }

    private async Task<Dictionary<int, RevisionResolution<DbSheetRowRevision>>> ResolveShownAsync(List<int> rowIds, CancellationToken cancellationToken)
    {
        var me = _currentUser.UserId;
        var revisions = await _sheetRowData.ListVisibleAsync(rowIds, me, cancellationToken);
        return RevisionResolver.Resolve(revisions, revision => revision.SheetRowId, me);
    }

    private void AddInheritedSettings(
        List<NewCell> newCells,
        Dictionary<int, RevisionResolution<DbSheetRowRevision>> shown,
        Dictionary<int, int> templateRowByRow,
        Dictionary<int, int> earlierByTemplateRow,
        List<ChosenSettings> chosen)
    {
        foreach (var cell in newCells)
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
                _cellValueData.AddSettings(mine.Id, cell.Id, match.Settings);
            }
        }
    }
}
