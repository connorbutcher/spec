using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Published;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Published;

public sealed class PublishedLookupService(
    PuSpecSheetDbContext db,
    PublishedVersionResolver resolver,
    PublishedLookupHitReader hitReader,
    PublishedStructureReader structureReader,
    PublishedLookupCellReader cellReader,
    PublishedKindValueReader valueReader) : IPublishedLookupService
{
    public async Task<PublishedLookupResolution> ResolveAsync(
        PublishedLookupCriteria criteria,
        Guid? sheetPublicId,
        PublishedVersionPoint point,
        CancellationToken cancellationToken)
    {
        List<PublishedLookupHit> hits;
        Dictionary<int, ResolvedSheetVersion> versions;

        if (sheetPublicId is { } publicId)
        {
            var version = await resolver.ResolveAsync(publicId, point, cancellationToken);
            hits = await hitReader.Query(criteria, version.PublishedAtUtc, version.SheetId).ToListAsync(cancellationToken);
            versions = new Dictionary<int, ResolvedSheetVersion> { [version.SheetId] = version };
        }
        else
        {
            var moment = point.AtUtc ?? DateTime.UtcNow;
            hits = await hitReader.Query(criteria, moment, null).ToListAsync(cancellationToken);
            versions = await VersionsAsync(hits, moment, cancellationToken);
            hits = hits.Where(hit => versions.ContainsKey(hit.SheetId)).ToList();
        }

        if (hits.Count == 0)
        {
            await EnsureKeyExistsAsync(criteria.Key, cancellationToken);
        }

        var ordered = hits
            .OrderBy(hit => hit.PhaseCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(hit => hit.SheetTypeId)
            .ThenBy(hit => hit.SheetId)
            .ThenBy(hit => hit.TableId)
            .ThenBy(hit => hit.CellId)
            .ToList();

        return new PublishedLookupResolution(criteria, ordered, versions);
    }

    public async Task<PublishedLookupDto> ReadAsync(PublishedLookupResolution resolution, CancellationToken cancellationToken)
    {
        var structures = new Dictionary<int, PublishedStructure>();
        var matches = new List<PublishedLookupMatchDto>();

        foreach (var hit in resolution.Hits)
        {
            var version = resolution.Versions[hit.SheetId];
            if (!structures.TryGetValue(hit.SheetId, out var structure))
            {
                structure = await structureReader.ReadAsync(version, cancellationToken);
                structures[hit.SheetId] = structure;
            }

            // The value is still stored for a cell in a section or column block that was later removed.
            if (!PublishedLookupAssembler.IsOnSheet(hit, structure))
            {
                continue;
            }

            var sectionIds = PublishedLookupAssembler.ScopeSections(hit, structure);
            var cells = await cellReader.Query(version, hit, sectionIds).ToListAsync(cancellationToken);
            var values = await valueReader.ReadAsync(version, cells, wholeSheet: false, cancellationToken);

            matches.Add(PublishedLookupAssembler.Assemble(hit, version, structure, cells, values));
        }

        return new PublishedLookupDto(resolution.Criteria.Key, resolution.Criteria.Value, matches);
    }

    /// <summary>The newest version of each sheet a value was found on, published at or before the moment.</summary>
    private async Task<Dictionary<int, ResolvedSheetVersion>> VersionsAsync(
        List<PublishedLookupHit> hits,
        DateTime moment,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, ResolvedSheetVersion>();
        if (hits.Count == 0)
        {
            return result;
        }

        var publicIds = hits
            .GroupBy(hit => hit.SheetId)
            .ToDictionary(group => group.Key, group => group.First().SheetPublicId);
        var sheetIds = publicIds.Keys.ToList();

        var versions = await db.SheetVersions
            .AsNoTracking()
            .Where(version => sheetIds.Contains(version.SheetId) && version.PublishedAtUtc <= moment)
            .Select(version => new { version.SheetId, version.VersionNumber, version.PublishedAtUtc })
            .ToListAsync(cancellationToken);

        foreach (var version in versions.OrderBy(version => version.VersionNumber))
        {
            result[version.SheetId] = new ResolvedSheetVersion(
                version.SheetId,
                publicIds[version.SheetId],
                version.VersionNumber,
                DateTime.SpecifyKind(version.PublishedAtUtc, DateTimeKind.Utc));
        }

        return result;
    }

    /// <summary>A key nobody has given to a cell is a mistake in the request, not an empty answer.</summary>
    private async Task EnsureKeyExistsAsync(string key, CancellationToken cancellationToken)
    {
        if (!await db.TemplateCells.AnyAsync(cell => cell.LookupKey == key, cancellationToken))
        {
            throw new InvalidRequestException($"No template cell has the lookup key \"{key}\".");
        }
    }
}
