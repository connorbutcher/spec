using PUSpecSheet.DataAccess.Common;

namespace PUSpecSheet.DataAccess.Templates;

// Keeps sheets' layouts stable: only the latest version can change, and only while no sheet table uses it.
public sealed class TemplateVersionGuard
{
    private readonly ITableTemplateDataService _tableTemplateData;

    public TemplateVersionGuard(ITableTemplateDataService tableTemplateData)
    {
        _tableTemplateData = tableTemplateData;
    }

    public async Task EnsureEditableAsync(int versionId, CancellationToken cancellationToken)
    {
        var version = await _tableTemplateData.FindVersionStateAsync(versionId, cancellationToken);

        if (version is null)
        {
            throw new NotFoundException($"Table template version {versionId} was not found.");
        }

        if (version.VersionNumber != version.LatestNumber)
        {
            throw new ConflictException(
                $"Version {version.VersionNumber} isn't the latest version. Changes go into version {version.LatestNumber}.");
        }

        if (version.IsInUse)
        {
            throw new ConflictException(
                $"Version {version.VersionNumber} is used on sheets, so it can't change. Create a new version to make changes.");
        }
    }

    public async Task EnsureTemplateUnusedAsync(int templateId, CancellationToken cancellationToken)
    {
        if (await _tableTemplateData.IsUsedAsync(templateId, cancellationToken))
        {
            throw new ConflictException("Sheets already use this table, so it can't be deleted.");
        }
    }
}
