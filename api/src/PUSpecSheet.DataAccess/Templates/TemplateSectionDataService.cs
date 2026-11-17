using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public sealed class TemplateSectionDataService : ITemplateSectionDataService
{
    private readonly PuSpecSheetDbContext _db;

    public TemplateSectionDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<DbTemplateSection>> ListAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateSections
            .AsNoTracking()
            .Where(section => section.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbTemplateSection>> ListWithRowsAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateSections
            .AsNoTracking()
            .Where(section => section.TableTemplateVersionId == versionId)
            .Include(section => section.Rows)
            .ThenInclude(row => row.Cells)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbTemplateSection>> ListForEditAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateSections
            .Where(candidate => candidate.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbTemplateSection>> ListSiblingsForEditAsync(int versionId, int? parentSectionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateSections
            .Where(candidate => candidate.TableTemplateVersionId == versionId
                && candidate.ParentSectionId == parentSectionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<DbTemplateSection?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.TemplateSections.FindAsync([id], cancellationToken);
    }

    public async Task<int?> FindVersionIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.TemplateSections
            .AsNoTracking()
            .Where(candidate => candidate.Id == id)
            .Select(candidate => (int?)candidate.TableTemplateVersionId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<SectionRole?> FindRoleAsync(int versionId, int id, CancellationToken cancellationToken)
    {
        return await _db.TemplateSections
            .AsNoTracking()
            .Where(section => section.Id == id && section.TableTemplateVersionId == versionId)
            .Select(section => (SectionRole?)section.Role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<int?> GetLastDisplayOrderAsync(int versionId, int? parentSectionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateSections
            .Where(section => section.TableTemplateVersionId == versionId
                && section.ParentSectionId == parentSectionId)
            .MaxAsync(section => (int?)section.DisplayOrder, cancellationToken);
    }

    public async Task<List<DbTemplateSection>> ListForLayoutAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TemplateSections
            .AsNoTracking()
            .Include(section => section.Rows)
            .ThenInclude(row => row.Cells)
            .Where(section => section.TableTemplateVersionId == versionId)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbTemplateSection>> ListInVersionsAsync(IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken)
    {
        return await _db.TemplateSections
            .AsNoTracking()
            .Where(section => versionIds.Contains(section.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
    }
}
