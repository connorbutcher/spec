using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Application.Templates;

/// <summary>
/// Loads a template at one version (sections, rows and cells) and returns it as the nested DTO. Every
/// template change returns this so the UI can replace its copy in one go.
/// </summary>
public sealed class TableTemplateReader
{
    private readonly ITableTemplateRepository templates;
    private readonly ITemplateSectionRepository sections;
    private readonly ITemplateRowRepository rows;
    private readonly ITemplateColumnBlockRepository columnBlocks;

    public TableTemplateReader(
        ITableTemplateRepository templates,
        ITemplateSectionRepository sections,
        ITemplateRowRepository rows,
        ITemplateColumnBlockRepository columnBlocks)
    {
        this.templates = templates;
        this.sections = sections;
        this.rows = rows;
        this.columnBlocks = columnBlocks;
    }

    /// <summary>Reads <paramref name="versionNumber"/>, or the latest version when it's null.</summary>
    public async Task<TableTemplateDto> ReadAsync(int templateId, int? versionNumber, CancellationToken cancellationToken)
    {
        var template = await templates.FindAsync(templateId, cancellationToken);

        if (template is null)
        {
            throw new NotFoundException($"Table template {templateId} was not found.");
        }

        var versions = await templates.ListVersionsAsync(templateId, cancellationToken);

        var version = versionNumber is null
            ? versions.LastOrDefault()
            : versions.SingleOrDefault(candidate => candidate.VersionNumber == versionNumber);

        if (version is null)
        {
            throw new NotFoundException($"Version {versionNumber} of table template {templateId} was not found.");
        }

        // The ids of the versions a sheet table is built from.
        var inUse = await templates.GetVersionIdsInUseAsync(versions.Select(candidate => candidate.Id).ToList(), cancellationToken);
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
            await sections.ListAsync(version.Id, cancellationToken),
            await rows.ListWithCellsAsync(version.Id, cancellationToken),
            await columnBlocks.ListAsync(version.Id, cancellationToken));
    }

    public async Task<TableTemplateDto> ReadVersionAsync(int versionId, CancellationToken cancellationToken)
    {
        var version = await templates.FindVersionKeyAsync(versionId, cancellationToken);

        if (version is null)
        {
            throw new NotFoundException($"Table template version {versionId} was not found.");
        }

        return await ReadAsync(version.TableTemplateId, version.VersionNumber, cancellationToken);
    }
}
