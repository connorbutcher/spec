using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Revisions;

/// <summary>
/// Reads a sheet from the revisions, as it stood at a moment, with or without the drafts laid over it, and the users
/// that wrote it. <see cref="SheetReader"/> maps the result to a sheet.
/// </summary>
/// <remarks>
/// The sheet is read in one split query: filtered includes bring back only the tables, groups and rows
/// shown at the version's moment and the revisions that show them, the values of those row revisions alone, and each
/// cell with its template cell, type and configuration; and the widget and note rows of those table and group
/// revisions with their items. Tables, groups, rows, cells, widgets and notes have the ids of their spec items;
/// workflow step actors, which the revisions don't keep as items, are numbered from 1 within the sheet. An item's
/// creator is who wrote the revision shown, as the revisions don't keep it per item, except a note's.
/// </remarks>
internal class SpecSheetRevisionReader
{
    private readonly PUSpecSheetDbContext _dbContext;

    public SpecSheetRevisionReader(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    // The sheet with what was shown at the moment: each table, group, row and property whose revision then doesn't
    // remove it, with that revision alone, and each external reference with its revision then, if it had one. Groups
    // and rows are only read under tables and groups that are shown, so nothing in a removed table is. With drafts,
    // each item's draft is read too, and is the revision shown where it has one (see SheetMapper).
    internal async Task<DbSheet> ReadSheetAsync(int sheetId, DateTime moment, bool withDrafts, CancellationToken cancellationToken)
    {
        return await _dbContext.Sheets
            .Where(s => s.Id == sheetId)
            .Include(s => s.SpecSheetPhase)
            .Include(s => s.SpecSheetType)

            .Include(s => s.Tables.Where(t => t.Revisions.Any(r => !r.IsDeleted
                    && ((withDrafts && r.Status == RevisionStatus.Draft)
                        || (r.Status == RevisionStatus.Published
                            && r.PublishedDateTime <= moment
                            && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))))
                .ThenInclude(t => t.Revisions.Where(r => (withDrafts && r.Status == RevisionStatus.Draft)
                    || (r.Status == RevisionStatus.Published
                        && r.PublishedDateTime <= moment
                        && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))

            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups.Where(g => g.Revisions.Any(r => !r.IsDeleted
                    && ((withDrafts && r.Status == RevisionStatus.Draft)
                        || (r.Status == RevisionStatus.Published
                            && r.PublishedDateTime <= moment
                            && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))))
                    .ThenInclude(g => g.Revisions.Where(r => (withDrafts && r.Status == RevisionStatus.Draft)
                        || (r.Status == RevisionStatus.Published
                            && r.PublishedDateTime <= moment
                            && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))

            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows.Where(row => row.Revisions.Any(r => !r.IsDeleted
                        && ((withDrafts && r.Status == RevisionStatus.Draft)
                            || (r.Status == RevisionStatus.Published
                                && r.PublishedDateTime <= moment
                                && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))))
                        .ThenInclude(row => row.Revisions.Where(r => (withDrafts && r.Status == RevisionStatus.Draft)
                            || (r.Status == RevisionStatus.Published
                                && r.PublishedDateTime <= moment
                                && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))
                            .ThenInclude(r => r.TextValues)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows)
                        .ThenInclude(row => row.Revisions)
                            .ThenInclude(r => r.NumericValues)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows)
                        .ThenInclude(row => row.Revisions)
                            .ThenInclude(r => r.PairValues)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows)
                        .ThenInclude(row => row.Revisions)
                            .ThenInclude(r => r.LinkedSheetValues)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows)
                        .ThenInclude(row => row.Revisions)
                            .ThenInclude(r => r.CellConfigurationOverrides)

            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows)
                        .ThenInclude(row => row.Cells)
                            .ThenInclude(c => c.TableCellTemplate)
                                .ThenInclude(tc => tc!.TableCellType)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows)
                        .ThenInclude(row => row.Cells)
                            .ThenInclude(c => c.TableCellTemplate)
                                .ThenInclude(tc => tc!.CellConfiguration)

            .Include(s => s.Tables)
                .ThenInclude(t => t.Revisions)
                    .ThenInclude(r => r.WidgetValues)
                        .ThenInclude(v => v.SheetTableWidget)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Revisions)
                    .ThenInclude(r => r.NoteValues)
                        .ThenInclude(v => v.SheetTableNote)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Revisions)
                        .ThenInclude(r => r.WidgetValues)
                            .ThenInclude(v => v.SheetTableGroupWidget)

            .Include(s => s.Properties.Where(p => p.Revisions.Any(r => !r.IsDeleted
                    && ((withDrafts && r.Status == RevisionStatus.Draft)
                        || (r.Status == RevisionStatus.Published
                            && r.PublishedDateTime <= moment
                            && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))))
                .ThenInclude(p => p.Revisions.Where(r => (withDrafts && r.Status == RevisionStatus.Draft)
                    || (r.Status == RevisionStatus.Published
                        && r.PublishedDateTime <= moment
                        && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))
            .Include(s => s.Properties)
                .ThenInclude(p => p.SpecSheetTypeProperty)

            .Include(s => s.ExternalReferences)
                .ThenInclude(e => e.Revisions.Where(r => !r.IsDeleted
                    && r.PublishedDateTime <= moment
                    && (r.SupersededDateTime == null || r.SupersededDateTime > moment)))

            .AsSplitQuery()
            .AsNoTracking()
            .FirstAsync(cancellationToken);
    }

    internal async Task<Dictionary<int, DbUser>> ReadUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken)
    {
        var wanted = userIds.Distinct().ToList();
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => wanted.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, cancellationToken);
    }
}
