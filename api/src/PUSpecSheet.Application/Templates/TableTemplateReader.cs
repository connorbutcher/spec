using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Templates.Queries;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Templates;

/// <summary>
/// Loads a template at one version (sections, rows and cells) and returns it as the nested DTO. Every
/// template change returns this so the UI can replace its copy in one go.
/// </summary>
public sealed class TableTemplateReader
{
    private readonly PuSpecSheetDbContext db;

    public TableTemplateReader(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    /// <summary>Reads <paramref name="versionNumber"/>, or the latest version when it's null.</summary>
    public async Task<TableTemplateDto> ReadAsync(int templateId, int? versionNumber, CancellationToken cancellationToken)
    {
        var template = await TableTemplateQueries.FindAsync(db, templateId, cancellationToken);

        if (template is null)
        {
            throw new NotFoundException($"Table template {templateId} was not found.");
        }

        var versions = await TableTemplateQueries.ListVersionsAsync(db, templateId, cancellationToken);

        var version = versionNumber is null
            ? versions.LastOrDefault()
            : versions.SingleOrDefault(candidate => candidate.VersionNumber == versionNumber);

        if (version is null)
        {
            throw new NotFoundException($"Version {versionNumber} of table template {templateId} was not found.");
        }

        // The ids of the versions a sheet table is built from.
        var inUse = await TableTemplateQueries.GetVersionIdsInUseAsync(db, versions.Select(candidate => candidate.Id).ToList(), cancellationToken);
        var summaries = versions
            .Select(candidate => new TableTemplateVersionSummaryDto(
                candidate.Id,
                candidate.VersionNumber,
                candidate.CreatedAtUtc,
                inUse.Contains(candidate.Id)))
            .ToList();
        var isEditable = version == versions[^1] && !inUse.Contains(version.Id);
        return template.ToDto(
            version,
            isEditable,
            summaries,
            await TemplateSectionQueries.ListAsync(db, version.Id, cancellationToken),
            await TemplateRowQueries.ListWithCellsAsync(db, version.Id, cancellationToken),
            await TemplateColumnBlockQueries.ListAsync(db, version.Id, cancellationToken));
    }

    public async Task<TableTemplateDto> ReadVersionAsync(int versionId, CancellationToken cancellationToken)
    {
        var version = await TableTemplateQueries.FindVersionKeyAsync(db, versionId, cancellationToken);

        if (version is null)
        {
            throw new NotFoundException($"Table template version {versionId} was not found.");
        }

        return await ReadAsync(version.TableTemplateId, version.VersionNumber, cancellationToken);
    }
}
