using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Published;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Published;

public sealed class PublishedSheetQueryService(
    PuSpecSheetDbContext db,
    PublishedVersionResolver resolver,
    PublishedStructureReader structureReader,
    PublishedCellReader cellReader,
    PublishedValueReader valueReader,
    PublishedSheetCache cache) : IPublishedSheetQueryService
{
    public async Task<PublishedSheetReferenceDto> FindAsync(string phaseCode, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .AsNoTracking()
            .Where(sheet => sheet.Phase.Code == phaseCode && sheet.SheetTypeId == sheetTypeId)
            .Select(sheet => new PublishedSheetReferenceDto(
                sheet.PublicId,
                sheet.Phase.Code,
                sheet.SheetTypeId,
                sheet.Versions.Max(version => (int?)version.VersionNumber)))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Phase {phaseCode} has no sheet of type {sheetTypeId}.");
    }

    public async Task<IReadOnlyList<PublishedVersionDto>> VersionsAsync(Guid sheetPublicId, CancellationToken cancellationToken)
    {
        var versions = await db.SheetVersions
            .AsNoTracking()
            .Where(version => version.Sheet.PublicId == sheetPublicId)
            .OrderBy(version => version.VersionNumber)
            .Select(version => new { version.VersionNumber, version.PublishedAtUtc, version.Note })
            .ToListAsync(cancellationToken);

        if (versions.Count == 0
            && !await db.Sheets.AnyAsync(sheet => sheet.PublicId == sheetPublicId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetPublicId} was not found.");
        }

        return versions
            .Select(version => new PublishedVersionDto(
                version.VersionNumber,
                DateTime.SpecifyKind(version.PublishedAtUtc, DateTimeKind.Utc),
                version.Note))
            .ToList();
    }

    public Task<ResolvedSheetVersion> ResolveAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken)
    {
        return resolver.ResolveAsync(sheetPublicId, point, cancellationToken);
    }

    public async Task<PublishedSheetDto> ReadAsync(ResolvedSheetVersion version, PublishedSheetSelection selection, CancellationToken cancellationToken)
    {
        var key = version.KeyFor(selection);
        if (cache.TryGet(key, out var cached) && cached is not null)
        {
            return cached;
        }

        var structure = await structureReader.ReadAsync(version, cancellationToken);
        var scope = selection.IsEverything ? null : structure.ScopeFor(selection);
        var cells = await cellReader.ReadAsync(version, scope, selection.IncludeLabels, cancellationToken);
        var values = await valueReader.ReadAsync(version, selection.IsEverything ? null : cells, cancellationToken);

        var sheet = PublishedSheetAssembler.Assemble(version, selection, structure, cells, values);
        cache.Set(key, sheet, cells.Count);
        return sheet;
    }
}
