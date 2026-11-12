using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates.Queries;

/// <summary>The sections of template versions.</summary>
public static class TemplateSectionQueries
{
    /// <summary>A version's sections, read only.</summary>
    public static async Task<List<TemplateSection>> ListAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Where(section => section.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>A version's sections with their rows and cells, read only.</summary>
    public static async Task<List<TemplateSection>> ListWithRowsAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Where(section => section.TableTemplateVersionId == versionId)
            .Include(section => section.Rows)
            .ThenInclude(row => row.Cells)
            .ToListAsync(cancellationToken);
    }

    /// <summary>A version's sections, tracked for editing.</summary>
    public static async Task<List<TemplateSection>> ListForEditAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .Where(candidate => candidate.TableTemplateVersionId == versionId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The sections under one parent (or at the top level) of a version, tracked for editing.</summary>
    public static async Task<List<TemplateSection>> ListSiblingsForEditAsync(PuSpecSheetDbContext db, int versionId, int? parentSectionId, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .Where(candidate => candidate.TableTemplateVersionId == versionId
                && candidate.ParentSectionId == parentSectionId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The section, tracked for editing; null when there is no such section.</summary>
    public static async Task<TemplateSection?> FindForEditAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.TemplateSections.FindAsync([id], cancellationToken);
    }

    /// <summary>The version the section is in; null when there is no such section.</summary>
    public static async Task<int?> FindVersionIdAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Where(candidate => candidate.Id == id)
            .Select(candidate => (int?)candidate.TableTemplateVersionId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>The role of a section of the version; null when the version has no such section.</summary>
    public static async Task<SectionRole?> FindRoleAsync(PuSpecSheetDbContext db, int versionId, int id, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Where(section => section.Id == id && section.TableTemplateVersionId == versionId)
            .Select(section => (SectionRole?)section.Role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>The highest display order under one parent (or at the top level) of a version, or null when there are none.</summary>
    public static async Task<int?> GetLastDisplayOrderAsync(PuSpecSheetDbContext db, int versionId, int? parentSectionId, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .Where(section => section.TableTemplateVersionId == versionId
                && section.ParentSectionId == parentSectionId)
            .MaxAsync(section => (int?)section.DisplayOrder, cancellationToken);
    }

    /// <summary>A version's sections with their rows and cells, read only, for laying sheet content out from it.</summary>
    public static async Task<List<TemplateSection>> ListForLayoutAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Include(section => section.Rows)
            .ThenInclude(row => row.Cells)
            .Where(section => section.TableTemplateVersionId == versionId)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    /// <summary>Every section of the versions, read only.</summary>
    public static async Task<List<TemplateSection>> ListInVersionsAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken)
    {
        return await db.TemplateSections
            .AsNoTracking()
            .Where(section => versionIds.Contains(section.TableTemplateVersionId))
            .ToListAsync(cancellationToken);
    }
}
