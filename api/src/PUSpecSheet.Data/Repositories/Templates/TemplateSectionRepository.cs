using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Data.Repositories.Templates;

public sealed class TemplateSectionRepository : ITemplateSectionRepository
{
    private readonly PuSpecSheetDbContext db;

    public TemplateSectionRepository(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<List<TemplateSection>> ListAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Where(section => section.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateSection>> ListWithRowsAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Where(section => section.TableTemplateVersionId == versionId)
            .Include(section => section.Rows)
            .ThenInclude(row => row.Cells)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateSection>> ListForEditAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .Where(candidate => candidate.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateSection>> ListSiblingsForEditAsync(int versionId, int? parentSectionId, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .Where(candidate => candidate.TableTemplateVersionId == versionId
                && candidate.ParentSectionId == parentSectionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<TemplateSection?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await db.TemplateSections.FindAsync([id], cancellationToken);
    }

    public async Task<int?> FindVersionIdAsync(int id, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Where(candidate => candidate.Id == id)
            .Select(candidate => (int?)candidate.TableTemplateVersionId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<SectionRole?> FindRoleAsync(int versionId, int id, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Where(section => section.Id == id && section.TableTemplateVersionId == versionId)
            .Select(section => (SectionRole?)section.Role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<int?> GetLastDisplayOrderAsync(int versionId, int? parentSectionId, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .Where(section => section.TableTemplateVersionId == versionId
                && section.ParentSectionId == parentSectionId)
            .MaxAsync(section => (int?)section.DisplayOrder, cancellationToken);
    }

    public async Task<List<TemplateSection>> ListForLayoutAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Include(section => section.Rows)
            .ThenInclude(row => row.Cells)
            .Where(section => section.TableTemplateVersionId == versionId)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateSection>> ListInVersionsAsync(IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Where(section => versionIds.Contains(section.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
    }

    public void Add(TemplateSection section)
    {
        db.TemplateSections.Add(section);
    }

    public void RemoveRange(IEnumerable<TemplateSection> sections)
    {
        db.TemplateSections.RemoveRange(sections);
    }
}
