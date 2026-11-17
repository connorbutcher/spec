using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database.Mappers.Templates;

namespace PUSpecSheet.DataAccess.Templates;

// Every template change returns the whole template so the UI can replace its copy in one go.
public sealed class TableTemplateReader
{
    private readonly ITableTemplateDataService _tableTemplateData;
    private readonly ITemplateSectionDataService _templateSectionData;
    private readonly ITemplateRowDataService _templateRowData;
    private readonly ITemplateColumnBlockDataService _templateColumnBlockData;

    public TableTemplateReader(
        ITableTemplateDataService tableTemplateData,
        ITemplateSectionDataService templateSectionData,
        ITemplateRowDataService templateRowData,
        ITemplateColumnBlockDataService templateColumnBlockData)
    {
        _tableTemplateData = tableTemplateData;
        _templateSectionData = templateSectionData;
        _templateRowData = templateRowData;
        _templateColumnBlockData = templateColumnBlockData;
    }

    public async Task<TableTemplate> ReadAsync(int templateId, int? versionNumber, CancellationToken cancellationToken)
    {
        var template = await _tableTemplateData.FindAsync(templateId, cancellationToken);

        if (template is null)
        {
            throw new NotFoundException($"Table template {templateId} was not found.");
        }

        var versions = await _tableTemplateData.ListVersionsAsync(templateId, cancellationToken);

        var version = versionNumber is null
            ? versions.LastOrDefault()
            : versions.SingleOrDefault(candidate => candidate.VersionNumber == versionNumber);

        if (version is null)
        {
            throw new NotFoundException($"Version {versionNumber} of table template {templateId} was not found.");
        }

        // The ids of the versions a sheet table is built from.
        var inUse = await _tableTemplateData.GetVersionIdsInUseAsync(versions.Select(candidate => candidate.Id).ToList(), cancellationToken);
        var summaries = versions
            .Select(candidate => candidate.ToSummary(inUse.Contains(candidate.Id)))
            .ToList();
        var isEditable = version == versions[^1] && !inUse.Contains(version.Id);
        return template.ToModel(
            version,
            isEditable,
            summaries,
            await _templateSectionData.ListAsync(version.Id, cancellationToken),
            await _templateRowData.ListWithCellsAsync(version.Id, cancellationToken),
            await _templateColumnBlockData.ListAsync(version.Id, cancellationToken));
    }

    public async Task<TableTemplate> ReadVersionAsync(int versionId, CancellationToken cancellationToken)
    {
        var version = await _tableTemplateData.FindVersionKeyAsync(versionId, cancellationToken);

        if (version is null)
        {
            throw new NotFoundException($"Table template version {versionId} was not found.");
        }

        return await ReadAsync(version.TableTemplateId, version.VersionNumber, cancellationToken);
    }
}
