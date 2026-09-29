using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Templates;

/// <summary>
/// Loads a template at one version (sections, rows and cells) and returns it as the nested DTO. Every
/// template change returns this so the UI can replace its copy in one go.
/// </summary>
public sealed class TableTemplateReader(PuSpecSheetDbContext db)
{
    /// <summary>Reads <paramref name="versionNumber"/>, or the latest version when it's null.</summary>
    public async Task<TableTemplateDto> ReadAsync(int templateId, int? versionNumber, CancellationToken cancellationToken)
    {
        var template = await db.TableTemplates
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == templateId, cancellationToken);

        if (template is null)
        {
            throw new NotFoundException($"Table template {templateId} was not found.");
        }

        var versions = await db.TableTemplateVersions
            .AsNoTracking()
            .Where(version => version.TableTemplateId == templateId)
            .OrderBy(version => version.VersionNumber)
            .ToListAsync(cancellationToken);

        var version = versionNumber is null
            ? versions.LastOrDefault()
            : versions.SingleOrDefault(candidate => candidate.VersionNumber == versionNumber);

        if (version is null)
        {
            throw new NotFoundException($"Version {versionNumber} of table template {templateId} was not found.");
        }

        var versionIds = versions.Select(candidate => candidate.Id).ToList();
        var inUse = await db.SheetTables
            .Where(table => versionIds.Contains(table.TableTemplateVersionId))
            .Select(table => table.TableTemplateVersionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var summaries = versions
            .Select(candidate => new TableTemplateVersionSummaryDto(
                candidate.Id,
                candidate.VersionNumber,
                candidate.CreatedAtUtc,
                inUse.Contains(candidate.Id)))
            .ToList();

        var sections = await db.TemplateSections
            .AsNoTracking()
            .Where(section => section.TableTemplateVersionId == version.Id)
            .ToListAsync(cancellationToken);

        var rows = await db.TemplateRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => row.TemplateSection.TableTemplateVersionId == version.Id)
            .ToListAsync(cancellationToken);

        var isEditable = version == versions[^1] && !inUse.Contains(version.Id);
        return template.ToDto(version, isEditable, summaries, sections, rows);
    }

    /// <summary>Reads the version with the given id.</summary>
    public async Task<TableTemplateDto> ReadVersionAsync(int versionId, CancellationToken cancellationToken)
    {
        var version = await db.TableTemplateVersions
            .AsNoTracking()
            .Where(candidate => candidate.Id == versionId)
            .Select(candidate => new { candidate.TableTemplateId, candidate.VersionNumber })
            .SingleOrDefaultAsync(cancellationToken);

        if (version is null)
        {
            throw new NotFoundException($"Table template version {versionId} was not found.");
        }

        return await ReadAsync(version.TableTemplateId, version.VersionNumber, cancellationToken);
    }
}
