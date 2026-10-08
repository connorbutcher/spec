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

        var cells = (await db.SheetCells
                .AsNoTracking()
                .Where(cell => newRowIds.Contains(cell.SheetRowId))
                .Select(cell => new { cell.Id, cell.SheetRowId, cell.TemplateCellId, cell.SheetColumnBlockId, cell.TemplateCell.CellType.Kind })
                .ToListAsync(cancellationToken))
            .Where(cell => cell.Kind.HasInstanceSettings())
            .ToList();
        if (cells.Count == 0)
        {
            return;
        }

        // Every row of the table built from the same template rows, as the user sees them.
        var newRows = cells.Select(cell => cell.SheetRowId).ToHashSet();
        var templateRowIds = await db.SheetRows
            .Where(row => newRows.Contains(row.Id))
            .Select(row => row.TemplateRowId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var templateRowByRow = await db.SheetRows
            .Where(row => row.SheetSection.SheetTableId == tableId && templateRowIds.Contains(row.TemplateRowId))
            .ToDictionaryAsync(row => row.Id, row => row.TemplateRowId, cancellationToken);

        var me = currentUser.UserId;
        var rowIds = templateRowByRow.Keys.ToList();
        var shown = RevisionResolver.Resolve(
            await db.SheetRowRevisions
                .Where(revision => rowIds.Contains(revision.SheetRowId))
                .VisibleTo(me)
                .AsNoTracking()
                .ToListAsync(cancellationToken),
            revision => revision.SheetRowId,
            me);

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
            .Select(value => new { value.SheetRowRevisionId, value.Settings, value.SheetCell.TemplateCellId, value.SheetCell.SheetColumnBlockId })
            .ToListAsync(cancellationToken);

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
}
