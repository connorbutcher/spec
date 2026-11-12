using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Data.Repositories.Templates;

public sealed class TableTemplateRepository : ITableTemplateRepository
{
    private readonly PuSpecSheetDbContext db;

    public TableTemplateRepository(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<List<TableTemplateSummaryDto>> ListSummariesAsync(int? sheetTypeId, CancellationToken cancellationToken)
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

    public async Task<TableTemplate?> FindAsync(int id, CancellationToken cancellationToken)
    {
        return await db.TableTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<TableTemplate?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await db.TableTemplates.FindAsync([id], cancellationToken);
    }

    public async Task<List<TableTemplate>> ListForEditAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.TableTemplates
            .Where(template => template.SheetTypeId == sheetTypeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameTakenAsync(int sheetTypeId, string name, int? exceptId, CancellationToken cancellationToken)
    {
        return await db.TableTemplates.AnyAsync(
            template => template.SheetTypeId == sheetTypeId && template.Name == name && template.Id != exceptId,
            cancellationToken);
    }

    public async Task<int?> GetLastDisplayOrderAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.TableTemplates
            .Where(template => template.SheetTypeId == sheetTypeId)
            .MaxAsync(template => (int?)template.DisplayOrder, cancellationToken);
    }

    public async Task<bool> IsUsedAsync(int templateId, CancellationToken cancellationToken)
    {
        return await db.SheetTables.AnyAsync(
            table => table.TableTemplateVersion.TableTemplateId == templateId,
            cancellationToken);
    }

    public async Task<List<TableTemplateVersion>> ListVersionsAsync(int templateId, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .AsNoTracking()
            .Where(version => version.TableTemplateId == templateId)
            .OrderBy(version => version.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<TableTemplateVersion> GetLatestVersionForEditAsync(int templateId, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .Where(version => version.TableTemplateId == templateId)
            .OrderByDescending(version => version.VersionNumber)
            .FirstAsync(cancellationToken);
    }

    public async Task<TemplateVersionKey?> FindVersionKeyAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .AsNoTracking()
            .Where(candidate => candidate.Id == versionId)
            .Select(candidate => new TemplateVersionKey(candidate.TableTemplateId, candidate.VersionNumber))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TemplateVersionState?> FindVersionStateAsync(int versionId, CancellationToken cancellationToken)
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

    public async Task<TemplateOrientation> GetOrientationAsync(int versionId, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .Where(version => version.Id == versionId)
            .Select(version => version.Orientation)
            .SingleAsync(cancellationToken);
    }

    public async Task<List<int>> GetVersionIdsInUseAsync(IReadOnlyCollection<int> versionIds, CancellationToken cancellationToken)
    {
        return await db.SheetTables
            .Where(table => versionIds.Contains(table.TableTemplateVersionId))
            .Select(table => table.TableTemplateVersionId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<TableTemplate?> FindForSheetTypeAsync(int id, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.TableTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id && candidate.SheetTypeId == sheetTypeId, cancellationToken);
    }

    public async Task<List<TableTemplate>> ListForSheetTypeAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.TableTemplates
            .AsNoTracking()
            .Where(template => template.SheetTypeId == sheetTypeId)
            .OrderBy(template => template.DisplayOrder)
            .ThenBy(template => template.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<TableTemplateVersion> GetLatestVersionAsync(int templateId, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .AsNoTracking()
            .Where(candidate => candidate.TableTemplateId == templateId)
            .OrderByDescending(candidate => candidate.VersionNumber)
            .FirstAsync(cancellationToken);
    }

    public async Task<Dictionary<int, int>> GetLatestVersionNumbersAsync(IReadOnlyCollection<int> templateIds, CancellationToken cancellationToken)
    {
        return await db.TableTemplateVersions
            .AsNoTracking()
            .Where(version => templateIds.Contains(version.TableTemplateId))
            .GroupBy(version => version.TableTemplateId)
            .Select(group => new { TemplateId = group.Key, Latest = group.Max(version => version.VersionNumber) })
            .ToDictionaryAsync(entry => entry.TemplateId, entry => entry.Latest, cancellationToken);
    }

    public void Add(TableTemplate tableTemplate)
    {
        db.TableTemplates.Add(tableTemplate);
    }

    public void Remove(TableTemplate tableTemplate)
    {
        db.TableTemplates.Remove(tableTemplate);
    }

    public void AddVersion(TableTemplateVersion version)
    {
        db.TableTemplateVersions.Add(version);
    }
}
