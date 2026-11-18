using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Undoes a user's checked-out changes to a sheet: drops their drafts that aren't awaiting approval (all of them, or
/// those of one table, group or row and what is inside it), so the items show their published revisions again and are
/// free for anyone to edit. Tables, groups, rows, cells, widgets and notes they added go with them. It runs in the
/// caller's transaction.
/// </summary>
internal class SheetDraftUndoer
{
    private readonly PUSpecSheetDbContext _dbContext;

    public SheetDraftUndoer(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    /// <summary>
    /// Undoes every change the user has checked out on the sheet. A sheet that is left with nothing, having never been
    /// published, is removed.
    /// </summary>
    public async Task UndoAllAsync(int sheetId, int userId, CancellationToken cancellationToken)
    {
        var tableIds = _dbContext.SheetTables
            .Where(t => t.SheetId == sheetId)
            .Select(t => t.Id);
        var groupIds = _dbContext.SheetTableGroups
            .Where(g => tableIds.Contains(g.SheetTableId))
            .Select(g => g.Id);
        var rowIds = _dbContext.SheetTableRows
            .Where(r => groupIds.Contains(r.SheetTableGroupId))
            .Select(r => r.Id);

        await UndoAsync(sheetId, userId, tableIds, groupIds, rowIds, cancellationToken);

        await _dbContext.SheetPropertyRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.CreatedByUserId == userId && r.SheetSubmissionId == null)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetLinkedTableRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.CreatedByUserId == userId && r.SheetSubmissionId == null)
            .ExecuteDeleteAsync(cancellationToken);

        await DeleteItemsWithoutRevisionsAsync(sheetId, cancellationToken);
        await DeleteSheetIfEmptyAsync(sheetId, cancellationToken);
    }

    /// <summary>
    /// Undoes the user's checked-out changes to one table, group or row, and to what is inside it.
    /// </summary>
    public async Task UndoItemAsync(int sheetId, int userId, SheetItemKind kind, int itemId, CancellationToken cancellationToken)
    {
        var tableIds = kind == SheetItemKind.Table
            ? _dbContext.SheetTables.Where(t => t.Id == itemId && t.SheetId == sheetId).Select(t => t.Id)
            : null;
        var groupIds = kind switch
        {
            SheetItemKind.Table => _dbContext.SheetTableGroups.Where(g => g.SheetTableId == itemId).Select(g => g.Id),
            SheetItemKind.Group => _dbContext.SheetTableGroups.Where(g => g.Id == itemId).Select(g => g.Id),
            _ => null
        };
        var rowIds = kind switch
        {
            SheetItemKind.Table => _dbContext.SheetTableRows.Where(r => _dbContext.SheetTableGroups.Any(g => g.Id == r.SheetTableGroupId && g.SheetTableId == itemId)).Select(r => r.Id),
            SheetItemKind.Group => _dbContext.SheetTableRows.Where(r => r.SheetTableGroupId == itemId).Select(r => r.Id),
            _ => _dbContext.SheetTableRows.Where(r => r.Id == itemId).Select(r => r.Id)
        };

        await UndoAsync(sheetId, userId, tableIds, groupIds, rowIds, cancellationToken);
        await DeleteItemsWithoutRevisionsAsync(sheetId, cancellationToken);
    }

    // Null means none of that kind is undone.
    private async Task UndoAsync(int sheetId, int userId, IQueryable<int>? tableIds, IQueryable<int>? groupIds, IQueryable<int> rowIds, CancellationToken cancellationToken)
    {
        var rowDrafts = _dbContext.SheetTableRowRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.CreatedByUserId == userId && r.SheetSubmissionId == null
                && rowIds.Contains(r.SheetTableRowId));

        await DeleteCellsAddedToPublishedRowsAsync(sheetId, rowDrafts.Select(r => r.SheetTableRowId), cancellationToken);
        await rowDrafts.ExecuteDeleteAsync(cancellationToken);

        if (groupIds is not null)
        {
            await _dbContext.SheetTableGroupRevisions
                .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.CreatedByUserId == userId && r.SheetSubmissionId == null
                    && groupIds.Contains(r.SheetTableGroupId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (tableIds is not null)
        {
            await _dbContext.SheetTableRevisions
                .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.CreatedByUserId == userId && r.SheetSubmissionId == null
                    && tableIds.Contains(r.SheetTableId))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }

    // The revisions don't keep cells per revision: a cell added to a published row in a draft is one made since the
    // sheet's last version was published. Its values are all in the row draft dropped with it.
    private async Task DeleteCellsAddedToPublishedRowsAsync(int sheetId, IQueryable<int> rowIds, CancellationToken cancellationToken)
    {
        var lastPublished = await _dbContext.SheetVersions
            .Where(v => v.SheetId == sheetId)
            .MaxAsync(v => (DateTime?)v.PublishedDateTime, cancellationToken);
        if (!lastPublished.HasValue)
        {
            return;
        }

        var addedCellIds = _dbContext.SheetTableCells
            .Where(c => c.CreatedDateTime > lastPublished.Value
                && rowIds.Contains(c.SheetTableRowId)
                && _dbContext.SheetTableRowRevisions.Any(r => r.SheetTableRowId == c.SheetTableRowId && r.Status == RevisionStatus.Published))
            .Select(c => c.Id);

        await _dbContext.SheetTextValues.Where(v => addedCellIds.Contains(v.SheetTableCellId)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetNumericValues.Where(v => addedCellIds.Contains(v.SheetTableCellId)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetPairValues.Where(v => addedCellIds.Contains(v.SheetTableCellId)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetLinkedSheetValues.Where(v => addedCellIds.Contains(v.SheetTableCellId)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetCellConfigurationOverrides.Where(v => addedCellIds.Contains(v.SheetTableCellId)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetTableCells.Where(c => addedCellIds.Contains(c.Id)).ExecuteDeleteAsync(cancellationToken);
    }

    // Items added in a dropped draft have no revisions left. Widgets and notes are items whose values are on their table's
    // or group's revisions, so one with no values left was added in a dropped draft too.
    private async Task DeleteItemsWithoutRevisionsAsync(int sheetId, CancellationToken cancellationToken)
    {
        await _dbContext.SheetTableWidgets
            .Where(w => !_dbContext.SheetTableWidgetValues.Any(v => v.SheetTableWidgetId == w.Id) && _dbContext.SheetTables.Any(t => t.Id == w.SheetTableId && t.SheetId == sheetId))
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetTableNotes
            .Where(n => !_dbContext.SheetTableNoteValues.Any(v => v.SheetTableNoteId == n.Id) && _dbContext.SheetTables.Any(t => t.Id == n.SheetTableId && t.SheetId == sheetId))
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetTableGroupWidgets
            .Where(w => !_dbContext.SheetTableGroupWidgetValues.Any(v => v.SheetTableGroupWidgetId == w.Id)
                && _dbContext.SheetTableGroups.Any(g => g.Id == w.SheetTableGroupId && _dbContext.SheetTables.Any(t => t.Id == g.SheetTableId && t.SheetId == sheetId)))
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetTableRows
            .Where(row => !row.Revisions.Any() && _dbContext.SheetTableGroups.Any(g => g.Id == row.SheetTableGroupId && _dbContext.SheetTables.Any(t => t.Id == g.SheetTableId && t.SheetId == sheetId)))
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetTableGroups
            .Where(g => !g.Revisions.Any() && _dbContext.SheetTables.Any(t => t.Id == g.SheetTableId && t.SheetId == sheetId))
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetTables
            .Where(t => t.SheetId == sheetId && !t.Revisions.Any())
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetProperties
            .Where(p => p.SheetId == sheetId && !p.Revisions.Any())
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetLinkedTables
            .Where(l => l.SheetId == sheetId && !l.Revisions.Any())
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task DeleteSheetIfEmptyAsync(int sheetId, CancellationToken cancellationToken)
    {
        await _dbContext.Sheets
            .Where(s => s.Id == sheetId
                && !_dbContext.SheetVersions.Any(v => v.SheetId == s.Id)
                && !_dbContext.SheetTables.Any(t => t.SheetId == s.Id)
                && !_dbContext.SheetProperties.Any(p => p.SheetId == s.Id)
                && !_dbContext.SheetLinkedTables.Any(l => l.SheetId == s.Id)
                && !_dbContext.SheetExternalReferences.Any(e => e.SheetId == s.Id)
                && !_dbContext.SheetSubmissions.Any(sub => sub.SheetId == s.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
