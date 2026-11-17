using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets;

public sealed class SheetRowDataService : ISheetRowDataService
{
    private readonly PuSpecSheetDbContext _db;

    public SheetRowDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<DbSheetRow>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _db.SheetRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => row.SheetSection.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    public async Task<DbSheetRow?> FindWithCellTypesAsync(int id, bool withOptions, CancellationToken cancellationToken)
    {
        var query = _db.SheetRows
            .AsNoTracking()
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.SheetTable);
        var withTypes = query.Include(candidate => candidate.Cells)
            .ThenInclude(cell => cell.TemplateCell)
            .ThenInclude(templateCell => templateCell.CellType);
        IQueryable<DbSheetRow> withCells = withOptions
            ? withTypes.ThenInclude(cellType => cellType.Options)
            : withTypes;
        return await withCells
            .AsSplitQuery()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<DbSheetRow?> FindWithSectionAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.SheetRows
            .AsNoTracking()
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.TemplateSection)
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.SheetTable)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<int?> FindSheetIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.SheetRows
            .Where(row => row.Id == id)
            .Select(row => (int?)row.SheetSection.SheetTable.SheetId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<int?> FindSectionIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.SheetRows
            .Where(row => row.Id == id)
            .Select(row => (int?)row.SheetSectionId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<SheetRowPlace?> FindPlaceAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.SheetRows
            .Where(row => row.Id == id)
            .Select(row => new SheetRowPlace { SheetSectionId = row.SheetSectionId, SheetTableId = row.SheetSection.SheetTableId })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsInSectionAsync(int sectionId, CancellationToken cancellationToken)
    {
        return await _db.SheetRows
            .Where(candidate => candidate.SheetSectionId == sectionId)
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsInSectionsAsync(IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken)
    {
        return await _db.SheetRows
            .Where(row => sectionIds.Contains(row.SheetSectionId))
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsOnTableAsync(int tableId, CancellationToken cancellationToken)
    {
        return await _db.SheetRows
            .Where(row => row.SheetSection.SheetTableId == tableId)
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListTemplateRowIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return await _db.SheetRows
            .Where(row => ids.Contains(row.Id))
            .Select(row => row.TemplateRowId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, int>> GetTemplateRowsOnTableAsync(int tableId, IReadOnlyCollection<int> templateRowIds, CancellationToken cancellationToken)
    {
        return await _db.SheetRows
            .Where(row => row.SheetSection.SheetTableId == tableId && templateRowIds.Contains(row.TemplateRowId))
            .ToDictionaryAsync(row => row.Id, row => row.TemplateRowId, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _db.SheetRows.Where(candidate => candidate.Id == id).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<List<DbSheetRowRevision>> ListVisibleInSectionAsync(int sectionId, int userId, CancellationToken cancellationToken)
    {
        return await _db.SheetRowRevisions
            .Where(revision => revision.SheetRow.SheetSectionId == sectionId)
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbSheetRowRevision>> ListVisibleAsync(IReadOnlyCollection<int> rowIds, int userId, CancellationToken cancellationToken)
    {
        return await _db.SheetRowRevisions
            .Where(revision => rowIds.Contains(revision.SheetRowId))
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbSheetRowRevision>> ListCurrentAndDraftAsync(int rowId, CancellationToken cancellationToken)
    {
        return await _db.SheetRowRevisions
            .Where(revision => revision.SheetRowId == rowId)
            .CurrentAndDrafts()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<DbSheetRowRevision?> FindDraftForEditAsync(int rowId, CancellationToken cancellationToken)
    {
        return await _db.SheetRowRevisions
            .SingleOrDefaultAsync(
                revision => revision.SheetRowId == rowId && revision.Status == RevisionStatus.Draft,
                cancellationToken);
    }

    public async Task<int?> FindDraftAuthorAsync(int rowId, CancellationToken cancellationToken)
    {
        return await _db.SheetRowRevisions
            .Where(revision => revision.SheetRowId == rowId && revision.Status == RevisionStatus.Draft)
            .Select(revision => (int?)revision.AuthorUserId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<List<ContainedHold>> ListHoldsInSectionsAsync(IReadOnlyCollection<int> sectionIds, int userId, CancellationToken cancellationToken)
    {
        return SheetRevisionQueries.ListHoldsAsync(_db.SheetRowRevisions.Where(revision => sectionIds.Contains(revision.SheetRow.SheetSectionId)), userId, cancellationToken);
    }

    public Task<List<ContainedHold>> ListHoldsOnTableAsync(int tableId, int userId, CancellationToken cancellationToken)
    {
        return SheetRevisionQueries.ListHoldsAsync(_db.SheetRowRevisions.Where(revision => revision.SheetRow.SheetSection.SheetTableId == tableId), userId, cancellationToken);
    }

    public async Task<List<DbSheetRowRevision>> ListCurrentOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _db.SheetRowRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetId == sheetId)
            .Current()
            .Where(revision => !revision.IsDeleted)
            .Include(revision => revision.SheetRow)
            .ToListAsync(cancellationToken);
    }

    public async Task SupersedeCurrentAsync(IReadOnlyCollection<int> rowIds, DateTime nowUtc, CancellationToken cancellationToken)
    {
        await _db.SheetRowRevisions.Where(revision => rowIds.Contains(revision.SheetRowId)).SupersedeCurrentAsync(nowUtc, cancellationToken);
    }
}
