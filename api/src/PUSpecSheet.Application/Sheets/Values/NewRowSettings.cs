using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Sheets.Values;

/// <summary>
/// Gives newly added rows the settings chosen for the row of the same kind that was added to the table
/// before them, so a column of linked dropdowns is pointed at its table once and not again for every row.
/// Only settings are carried over, never values.
/// </summary>
public sealed class NewRowSettings
{
    private readonly PuSpecSheetDbContext db;
    private readonly ICurrentUser currentUser;

    public NewRowSettings(
        PuSpecSheetDbContext db,
        ICurrentUser currentUser)
    {
        this.db = db;
        this.currentUser = currentUser;
    }

    /// <param name="tableId">The table the rows were added to.</param>
    /// <param name="newRowIds">The rows just added, which are still their author's first drafts.</param>
    /// <param name="cancellationToken">Stops the queries.</param>
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
        var chosen = await RowValueQueries.ListChosenSettingsAsync(db, earlierRevisionIds, cancellationToken);

        AddInheritedSettings(newCells, shown, templateRowByRow, earlierByTemplateRow, chosen);
        await db.SaveSheetChangesAsync(cancellationToken);
    }

    /// <summary>The same for the rows of a newly added section and of the sub-sections it starts with.</summary>
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
        var rowCells = await SheetCellQueries.ListNewCellsAsync(db, newRowIds, cancellationToken);
        return rowCells.Where(cell => cell.Kind.HasInstanceSettings()).ToList();
    }

    /// <summary>Every row of the table built from the same template rows as the new cells' rows, with that template row.</summary>
    private async Task<Dictionary<int, int>> LoadTemplateRowsAsync(int tableId, List<NewCell> newCells, CancellationToken cancellationToken)
    {
        var newRows = newCells.Select(cell => cell.SheetRowId).ToHashSet();
        var templateRowIds = await SheetRowQueries.ListTemplateRowIdsAsync(db, newRows, cancellationToken);
        return await SheetRowQueries.GetTemplateRowsOnTableAsync(db, tableId, templateRowIds, cancellationToken);
    }

    private async Task<Dictionary<int, RevisionResolution<DbSheetRowRevision>>> ResolveShownAsync(List<int> rowIds, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var revisions = await SheetRowRevisionQueries.ListVisibleAsync(db, rowIds, me, cancellationToken);
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
                RowValueQueries.AddSettings(db, mine.Id, cell.Id, match.Settings);
            }
        }
    }
}
