using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates;

public sealed class TableTemplateDataService : ITableTemplateDataService
{
    private readonly PuSpecSheetDbContext _db;

    public TableTemplateDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<TableTemplateSummary>> ListSummariesAsync(int? sheetTypeId, CancellationToken cancellationToken)
    {
        var query = _db.TableTemplates.AsNoTracking();
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
            .Select(entry => new TableTemplateSummary
            {
                Id = entry.template.Id,
                SheetTypeId = entry.template.SheetTypeId,
                Name = entry.template.Name,
                DisplayOrder = entry.template.DisplayOrder,
                LatestVersionNumber = entry.Latest.VersionNumber,
                Orientation = entry.Latest.Orientation,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DbTableTemplate?> FindAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.TableTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<DbTableTemplate?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.TableTemplates.FindAsync([id], cancellationToken);
    }

    public async Task<List<DbTableTemplate>> ListForEditAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        return await _db.TableTemplates
            .Where(template => template.SheetTypeId == sheetTypeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameTakenAsync(int sheetTypeId, string name, int? exceptId, CancellationToken cancellationToken)
    {
        return await _db.TableTemplates.AnyAsync(
            template => template.SheetTypeId == sheetTypeId && template.Name == name && template.Id != exceptId,
            cancellationToken);
    }

    public async Task<int?> GetLastDisplayOrderAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        return await _db.TableTemplates
            .Where(template => template.SheetTypeId == sheetTypeId)
            .MaxAsync(template => (int?)template.DisplayOrder, cancellationToken);
    }

    public async Task<bool> IsUsedAsync(int templateId, CancellationToken cancellationToken)
    {
        return await _db.SheetTables.AnyAsync(
            table => table.TableTemplateVersion.TableTemplateId == templateId,
            cancellationToken);
    }

    public async Task<List<DbTableTemplateVersion>> ListVersionsAsync(int templateId, CancellationToken cancellationToken)
    {
        return await _db.TableTemplateVersions
            .AsNoTracking()
            .Where(version => version.TableTemplateId == templateId)
            .OrderBy(version => version.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<DbTableTemplateVersion> GetLatestVersionForEditAsync(int templateId, CancellationToken cancellationToken)
    {
        return await _db.TableTemplateVersions
            .Where(version => version.TableTemplateId == templateId)
            .OrderByDescending(version => version.VersionNumber)
            .FirstAsync(cancellationToken);
    }

    public async Task<TemplateVersionKey?> FindVersionKeyAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TableTemplateVersions
            .AsNoTracking()
            .Where(candidate => candidate.Id == versionId)
            .Select(candidate => new TemplateVersionKey
            {
                TableTemplateId = candidate.TableTemplateId,
                VersionNumber = candidate.VersionNumber,
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TemplateVersionState?> FindVersionStateAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TableTemplateVersions
            .AsNoTracking()
            .Where(candidate => candidate.Id == versionId)
            .Select(candidate => new TemplateVersionState
            {
                VersionNumber = candidate.VersionNumber,
                LatestNumber = candidate.TableTemplate.Versions.Max(other => other.VersionNumber),
                IsInUse = _db.SheetTables.Any(table => table.TableTemplateVersionId == candidate.Id),
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TemplateOrientation> GetOrientationAsync(int versionId, CancellationToken cancellationToken)
    {
        return await _db.TableTemplateVersions
            .Where(version => version.Id == versionId)
            .Select(version => version.Orientation)
            .SingleAsync(cancellationToken);
    }

    public async Task<List<int>> GetVersionIdsInUseAsync(IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken)
    {
        return await _db.SheetTables
            .Where(table => versionIds.Contains(table.TableTemplateVersionId))
            .Select(table => table.TableTemplateVersionId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<DbTableTemplate?> FindForSheetTypeAsync(int id, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await _db.TableTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id && candidate.SheetTypeId == sheetTypeId, cancellationToken);
    }

    public async Task<List<DbTableTemplate>> ListForSheetTypeAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        return await _db.TableTemplates
            .AsNoTracking()
            .Where(template => template.SheetTypeId == sheetTypeId)
            .OrderBy(template => template.DisplayOrder)
            .ThenBy(template => template.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<DbTableTemplateVersion> GetLatestVersionAsync(int templateId, CancellationToken cancellationToken)
    {
        return await _db.TableTemplateVersions
            .AsNoTracking()
            .Where(candidate => candidate.TableTemplateId == templateId)
            .OrderByDescending(candidate => candidate.VersionNumber)
            .FirstAsync(cancellationToken);
    }

    public async Task<Dictionary<int, int>> GetLatestVersionNumbersAsync(IReadOnlyCollection<int> templateIds, CancellationToken cancellationToken)
    {
        return await _db.TableTemplateVersions
            .AsNoTracking()
            .Where(version => templateIds.Contains(version.TableTemplateId))
            .GroupBy(version => version.TableTemplateId)
            .Select(group => new { TemplateId = group.Key, Latest = group.Max(version => version.VersionNumber) })
            .ToDictionaryAsync(entry => entry.TemplateId, entry => entry.Latest, cancellationToken);
    }
}
