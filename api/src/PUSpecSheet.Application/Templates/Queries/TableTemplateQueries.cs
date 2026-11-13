using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Database;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates.Queries;

/// <summary>Table templates and their versions.</summary>
public static class TableTemplateQueries
{
    /// <summary>A summary of every template, or of one sheet type's, with its latest version.</summary>
    public static async Task<List<TableTemplateSummaryDto>> ListSummariesAsync(PuSpecSheetDbContext db, int? sheetTypeId, CancellationToken cancellationToken)
    {
        var query = db.TableTemplates.AsNoTracking();
        if (sheetTypeId is not null)
        {
            query = query.Where(template => template.SheetTypeId == sheetTypeId);
        }

        return await query
            .OrderBy(template => template.SheetTypeId)
            .ThenBy(template => template.DisplayOrder)
            .ThenBy(template => template.Name)
            .Select(template => new
            {
                template,
                Latest = template.Versions.OrderByDescending(version => version.VersionNumber).First(),
            })
            .Select(entry => new TableTemplateSummaryDto(
                entry.template.Id,
                entry.template.SheetTypeId,
                entry.template.Name,
                entry.template.DisplayOrder,
                entry.Latest.VersionNumber,
                entry.Latest.Orientation))
            .ToListAsync(cancellationToken);
    }

    /// <summary>The template, read only; null when there is no such template.</summary>
    public static async Task<DbTableTemplate?> FindAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.TableTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    /// <summary>The template, tracked for editing; null when there is no such template.</summary>
    public static async Task<DbTableTemplate?> FindForEditAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.TableTemplates.FindAsync([id], cancellationToken);
    }

    /// <summary>A sheet type's templates, tracked for editing.</summary>
    public static async Task<List<DbTableTemplate>> ListForEditAsync(PuSpecSheetDbContext db, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.TableTemplates
            .Where(template => template.SheetTypeId == sheetTypeId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Whether another template of the sheet type than <paramref name="exceptId"/> already has the name.</summary>
    public static async Task<bool> NameTakenAsync(PuSpecSheetDbContext db, int sheetTypeId, string name, int? exceptId, CancellationToken cancellationToken)
    {
        return await db.TableTemplates.AnyAsync(
            template => template.SheetTypeId == sheetTypeId && template.Name == name && template.Id != exceptId,
            cancellationToken);
    }

    /// <summary>The highest display order among a sheet type's templates, or null when it has none.</summary>
    public static async Task<int?> GetLastDisplayOrderAsync(PuSpecSheetDbContext db, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.TableTemplates
            .Where(template => template.SheetTypeId == sheetTypeId)
            .MaxAsync(template => (int?)template.DisplayOrder, cancellationToken);
    }

    /// <summary>Whether any sheet table is built from any version of the template.</summary>
    public static async Task<bool> IsUsedAsync(PuSpecSheetDbContext db, int templateId, CancellationToken cancellationToken)
    {
        return await db.SheetTables.AnyAsync(
            table => table.TableTemplateVersion.TableTemplateId == templateId,
            cancellationToken);
    }

    /// <summary>The template's versions, read only, oldest first.</summary>
    public static async Task<List<DbTableTemplateVersion>> ListVersionsAsync(PuSpecSheetDbContext db, int templateId, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .AsNoTracking()
            .Where(version => version.TableTemplateId == templateId)
            .OrderBy(version => version.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The template's latest version, tracked for editing.</summary>
    public static async Task<DbTableTemplateVersion> GetLatestVersionForEditAsync(PuSpecSheetDbContext db, int templateId, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .Where(version => version.TableTemplateId == templateId)
            .OrderByDescending(version => version.VersionNumber)
            .FirstAsync(cancellationToken);
    }

    /// <summary>Which template the version belongs to and its number; null when there is no such version.</summary>
    public static async Task<TemplateVersionKey?> FindVersionKeyAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .AsNoTracking()
            .Where(candidate => candidate.Id == versionId)
            .Select(candidate => new TemplateVersionKey(candidate.TableTemplateId, candidate.VersionNumber))
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>Whether the version can change; null when there is no such version.</summary>
    public static async Task<TemplateVersionState?> FindVersionStateAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .AsNoTracking()
            .Where(candidate => candidate.Id == versionId)
            .Select(candidate => new TemplateVersionState(
                candidate.VersionNumber,
                candidate.TableTemplate.Versions.Max(other => other.VersionNumber),
                db.SheetTables.Any(table => table.TableTemplateVersionId == candidate.Id)))
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>The version's orientation. The version must exist.</summary>
    public static async Task<TemplateOrientation> GetOrientationAsync(PuSpecSheetDbContext db, int versionId, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .Where(version => version.Id == versionId)
            .Select(version => version.Orientation)
            .SingleAsync(cancellationToken);
    }

    /// <summary>Which of the versions a sheet table is built from.</summary>
    public static async Task<List<int>> GetVersionIdsInUseAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken)
    {
        return await db.SheetTables
            .Where(table => versionIds.Contains(table.TableTemplateVersionId))
            .Select(table => table.TableTemplateVersionId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    /// <summary>The template, read only, when it is for the sheet type; null otherwise.</summary>
    public static async Task<DbTableTemplate?> FindForSheetTypeAsync(PuSpecSheetDbContext db, int id, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.TableTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id && candidate.SheetTypeId == sheetTypeId, cancellationToken);
    }

    /// <summary>A sheet type's templates, read only, in display order then name.</summary>
    public static async Task<List<DbTableTemplate>> ListForSheetTypeAsync(PuSpecSheetDbContext db, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.TableTemplates
            .AsNoTracking()
            .Where(template => template.SheetTypeId == sheetTypeId)
            .OrderBy(template => template.DisplayOrder)
            .ThenBy(template => template.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The template's latest version, read only. The template must have one.</summary>
    public static async Task<DbTableTemplateVersion> GetLatestVersionAsync(PuSpecSheetDbContext db, int templateId, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .AsNoTracking()
            .Where(candidate => candidate.TableTemplateId == templateId)
            .OrderByDescending(candidate => candidate.VersionNumber)
            .FirstAsync(cancellationToken);
    }

    /// <summary>The latest version number of each of the templates, by template id.</summary>
    public static async Task<Dictionary<int, int>> GetLatestVersionNumbersAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> templateIds, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .AsNoTracking()
            .Where(version => templateIds.Contains(version.TableTemplateId))
            .GroupBy(version => version.TableTemplateId)
            .Select(group => new { TemplateId = group.Key, Latest = group.Max(version => version.VersionNumber) })
            .ToDictionaryAsync(entry => entry.TemplateId, entry => entry.Latest, cancellationToken);
    }
}
