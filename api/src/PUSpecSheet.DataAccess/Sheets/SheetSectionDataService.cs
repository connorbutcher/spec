using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets;

public sealed class SheetSectionDataService : ISheetSectionDataService
{
    private readonly PuSpecSheetDbContext _db;

    public SheetSectionDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<DbSheetSection>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _db.SheetSections
            .AsNoTracking()
            .Include(section => section.TemplateSection)
            .Where(section => section.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    public async Task<DbSheetSection?> FindWithTemplateAndTableAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.SheetSections
            .AsNoTracking()
            .Include(section => section.TemplateSection)
            .Include(section => section.SheetTable)
            .SingleOrDefaultAsync(section => section.Id == id, cancellationToken);
    }

    public async Task<DbSheetSection?> FindOnTableAsync(int id, int tableId, CancellationToken cancellationToken)
    {
        return await _db.SheetSections
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id && candidate.SheetTableId == tableId, cancellationToken);
    }

    public async Task<List<SheetSectionPlace>> ListPlacesOnTableAsync(int tableId, CancellationToken cancellationToken)
    {
        return await _db.SheetSections
            .AsNoTracking()
            .Where(section => section.SheetTableId == tableId)
            .Select(section => new SheetSectionPlace
            {
                Id = section.Id,
                TemplateSectionId = section.TemplateSectionId,
                ParentSheetSectionId = section.ParentSheetSectionId,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, int?>> GetParentsOnTableAsync(int tableId, CancellationToken cancellationToken)
    {
        return await _db.SheetSections
            .AsNoTracking()
            .Where(section => section.SheetTableId == tableId)
            .Select(section => new { section.Id, section.ParentSheetSectionId })
            .ToDictionaryAsync(section => section.Id, section => section.ParentSheetSectionId, cancellationToken);
    }

    public async Task<Dictionary<int, int?>> GetParentsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return await _db.SheetSections
            .AsNoTracking()
            .Where(candidate => ids.Contains(candidate.Id))
            .Select(candidate => new { candidate.Id, candidate.ParentSheetSectionId })
            .ToDictionaryAsync(entry => entry.Id, entry => entry.ParentSheetSectionId, cancellationToken);
    }

    public async Task<List<int>> ListSiblingIdsAsync(int tableId, int? parentSectionId, CancellationToken cancellationToken)
    {
        return await _db.SheetSections
            .Where(candidate => candidate.SheetTableId == tableId && candidate.ParentSheetSectionId == parentSectionId)
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        await _db.SheetSections.Where(candidate => ids.Contains(candidate.Id)).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<List<DbSheetSectionRevision>> ListVisibleOnTableAsync(int tableId, int userId, CancellationToken cancellationToken)
    {
        return await _db.SheetSectionRevisions
            .Where(revision => revision.SheetSection.SheetTableId == tableId)
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SectionRemoval>> ListPendingRemovalsOnTableAsync(int tableId, CancellationToken cancellationToken)
    {
        return await _db.SheetSectionRevisions
            .Where(revision => revision.SheetSection.SheetTableId == tableId && revision.Status == RevisionStatus.Draft && revision.IsDeleted)
            .Select(revision => new SectionRemoval
            {
                SheetSectionId = revision.SheetSectionId,
                AuthorUserId = revision.AuthorUserId,
                AwaitingApproval = revision.SheetSubmissionId != null,
            })
            .ToListAsync(cancellationToken);
    }

    public Task<List<ContainedHold>> ListHoldsAsync(IReadOnlyCollection<int> sectionIds, int userId, CancellationToken cancellationToken)
    {
        return SheetRevisionQueries.ListHoldsAsync(_db.SheetSectionRevisions.Where(revision => sectionIds.Contains(revision.SheetSectionId)), userId, cancellationToken);
    }

    public Task<List<ContainedHold>> ListHoldsOnTableAsync(int tableId, int userId, CancellationToken cancellationToken)
    {
        return SheetRevisionQueries.ListHoldsAsync(_db.SheetSectionRevisions.Where(revision => revision.SheetSection.SheetTableId == tableId), userId, cancellationToken);
    }

    public async Task<List<DbSheetSectionRevision>> ListCurrentOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _db.SheetSectionRevisions
            .AsNoTracking()
            .Where(revision => revision.SheetId == sheetId)
            .Current()
            .Where(revision => !revision.IsDeleted)
            .Include(revision => revision.SheetSection)
            .ToListAsync(cancellationToken);
    }

    public async Task SupersedeCurrentAsync(IReadOnlyCollection<int> sectionIds, DateTime nowUtc, CancellationToken cancellationToken)
    {
        await _db.SheetSectionRevisions.Where(revision => sectionIds.Contains(revision.SheetSectionId)).SupersedeCurrentAsync(nowUtc, cancellationToken);
    }
}
