using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Templates.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Templates;

/// <summary>
/// Keeps sheets' layouts stable: only a template's latest version can change, and only while no sheet
/// table uses it. Changes to a version in use go into a new version instead.
/// </summary>
public sealed class TemplateVersionGuard
{
    private readonly PuSpecSheetDbContext db;

    public TemplateVersionGuard(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task EnsureEditableAsync(int versionId, CancellationToken cancellationToken)
    {
        var version = await TableTemplateQueries.FindVersionStateAsync(db, versionId, cancellationToken);

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
        if (await TableTemplateQueries.IsUsedAsync(db, templateId, cancellationToken))
        {
            throw new ConflictException("Sheets already use this table, so it can't be deleted.");
        }
    }
}
