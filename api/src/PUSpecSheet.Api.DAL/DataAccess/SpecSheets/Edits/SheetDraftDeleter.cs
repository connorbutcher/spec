using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Drops every user's checked-out changes to a sheet (an admin clearing its checkouts): every draft revision that isn't
/// awaiting approval, and the tables, groups, rows, cells, widgets, notes and properties added in them. Drafts awaiting
/// approval are left to be decided. It runs in the caller's transaction.
/// </summary>
internal class SheetDraftDeleter
{
    private readonly PUSpecSheetDbContext _dbContext;

    public SheetDraftDeleter(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    public async Task DeleteDraftsAsync(int sheetId, CancellationToken cancellationToken)
    {
        await DeleteCellsAddedSinceLastVersionAsync(sheetId, cancellationToken);

        await _dbContext.SheetTableRowRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.SheetSubmissionId == null)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetTableGroupRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.SheetSubmissionId == null)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetTableRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.SheetSubmissionId == null)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetPropertyRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.SheetSubmissionId == null)
            .ExecuteDeleteAsync(cancellationToken);

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
    }

    // The revisions don't keep cells per revision: a cell added to a published row in the draft is one made since the
    // sheet's last version was published. Its values are all in the row drafts dropped with it.
    private async Task DeleteCellsAddedSinceLastVersionAsync(int sheetId, CancellationToken cancellationToken)
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
                && _dbContext.SheetTableRowRevisions.Any(r => r.SheetTableRowId == c.SheetTableRowId && r.SheetId == sheetId && r.Status == RevisionStatus.Published)
                && _dbContext.SheetTableRowRevisions.Any(r => r.SheetTableRowId == c.SheetTableRowId && r.Status == RevisionStatus.Draft && r.SheetSubmissionId == null))
            .Select(c => c.Id);

        await _dbContext.SheetTextValues.Where(v => addedCellIds.Contains(v.SheetTableCellId)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetNumericValues.Where(v => addedCellIds.Contains(v.SheetTableCellId)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetPairValues.Where(v => addedCellIds.Contains(v.SheetTableCellId)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetLinkedSheetValues.Where(v => addedCellIds.Contains(v.SheetTableCellId)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetCellConfigurationOverrides.Where(v => addedCellIds.Contains(v.SheetTableCellId)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SheetTableCells.Where(c => addedCellIds.Contains(c.Id)).ExecuteDeleteAsync(cancellationToken);
    }
}