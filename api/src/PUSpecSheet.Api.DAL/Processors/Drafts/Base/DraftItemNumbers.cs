using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Base;

/// <summary>
/// Finds a table's or group's widget, or a table's note, by the id the draft read gives it. The revisions keep these
/// inside their table's or group's revision, so the read numbers each kind from 1 within the sheet: over the shown
/// tables in order of id, and their shown groups in order of id, in the order each revision holds them. The numbers are
/// worked out from the saved drafts, which are what the ids were read from.
/// </summary>
internal class DraftItemNumbers
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly int _sheetId;

    public DraftItemNumbers(PUSpecSheetDbContext dbContext, int sheetId)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
        _sheetId = sheetId;
    }

    public async Task<DraftItemNumber> FindTableWidgetAsync(int number, CancellationToken ct)
    {
        var tables = await ReadShownTablesAsync(ct);
        return Find(tables.Select(table => (table.SheetTableId, table.WidgetValues?.Count ?? 0)), number, "table widget");
    }

    public async Task<DraftItemNumber> FindNoteAsync(int number, CancellationToken ct)
    {
        var tables = await ReadShownTablesAsync(ct);
        return Find(tables.Select(table => (table.SheetTableId, table.Notes?.Count ?? 0)), number, "note");
    }

    public async Task<DraftItemNumber> FindGroupWidgetAsync(int number, CancellationToken ct)
    {
        var tableIds = (await ReadShownTablesAsync(ct)).Select(table => table.SheetTableId).ToList();
        var groups = await _dbContext.SheetTableGroupRevisions
            .AsNoTracking()
            .Where(r => r.SheetId == _sheetId && (r.Status == RevisionStatus.Draft || r.SupersededDateTime == null))
            .Join(_dbContext.SheetTableGroups, r => r.SheetTableGroupId, g => g.Id, (r, g) => new { Revision = r, g.SheetTableId })
            .ToListAsync(ct);

        var shown = groups
            .GroupBy(g => g.Revision.SheetTableGroupId)
            .Select(g => new { Revision = Shown(g.Select(x => x.Revision)), g.First().SheetTableId })
            .Where(g => !g.Revision.IsDeleted && tableIds.Contains(g.SheetTableId))
            .OrderBy(g => tableIds.IndexOf(g.SheetTableId))
            .ThenBy(g => g.Revision.SheetTableGroupId)
            .Select(g => (g.Revision.SheetTableGroupId, g.Revision.WidgetValues?.Count ?? 0));

        return Find(shown, number, "group widget");
    }

    private DraftItemNumber Find(IEnumerable<(int ItemId, int Count)> items, int number, string kind)
    {
        var first = 1;
        foreach (var (itemId, count) in items)
        {
            if (number < first + count)
            {
                return new DraftItemNumber { ItemId = itemId, Index = number - first };
            }

            first += count;
        }

        throw new KeyNotFoundException($"No {kind} with Id {number} on Sheet with Id {_sheetId}.");
    }

    private static TRevision Shown<TRevision>(IEnumerable<TRevision> revisions)
        where TRevision : IDbSheetRevision
    {
        return revisions.OrderBy(revision => revision.Status == RevisionStatus.Draft ? 0 : 1).First();
    }

    private async Task<List<DbSheetTableRevision>> ReadShownTablesAsync(CancellationToken ct)
    {
        var revisions = await _dbContext.SheetTableRevisions
            .AsNoTracking()
            .Where(r => r.SheetId == _sheetId && (r.Status == RevisionStatus.Draft || r.SupersededDateTime == null))
            .ToListAsync(ct);

        return [.. revisions
            .GroupBy(r => r.SheetTableId)
            .Select(Shown)
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.SheetTableId)];
    }
}