using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Templates;

/// <summary>
/// Keeps sheets' layouts stable: only a template's latest version can change, and only while no sheet
/// table uses it. Changes to a version in use go into a new version instead.
/// </summary>
public sealed class TemplateVersionGuard(PuSpecSheetDbContext db)
{
    public async Task EnsureEditableAsync(int versionId, CancellationToken cancellationToken)
    {
        var version = await db.TableTemplateVersions
            .AsNoTracking()
            .Where(candidate => candidate.Id == versionId)
            .Select(candidate => new
            {
                candidate.VersionNumber,
                LatestNumber = candidate.TableTemplate.Versions.Max(other => other.VersionNumber),
                IsInUse = db.SheetTables.Any(table => table.TableTemplateVersionId == candidate.Id),
            })
            .SingleOrDefaultAsync(cancellationToken);

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
        var inUse = await db.SheetTables.AnyAsync(
            table => table.TableTemplateVersion.TableTemplateId == templateId,
            cancellationToken);

        if (inUse)
        {
            throw new ConflictException("Sheets already use this table, so it can't be deleted.");
        }
    }
}
