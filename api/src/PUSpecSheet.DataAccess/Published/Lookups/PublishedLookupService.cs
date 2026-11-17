using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Published.Queries;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.DataAccess.Templates.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Published.Lookups;

public sealed class PublishedLookupService : IPublishedLookupService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly PublishedVersionResolver _resolver;
    private readonly PublishedLookupCache _cache;

    public PublishedLookupService(
        PuSpecSheetDbContext db,
        PublishedVersionResolver resolver,
        PublishedLookupCache cache)
    {
        _db = db;
        _resolver = resolver;
        _cache = cache;
    }

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
            var version = await _resolver.ResolveAsync(publicId, point, cancellationToken);
            hits = await PublishedLookupHitQueries.ListAsync(_db, criteria, version.PublishedAtUtc, version.SheetId, cancellationToken);
            versions = new Dictionary<int, ResolvedSheetVersion> { [version.SheetId] = version };
        }
        else
        {
            var moment = point.AtUtc ?? DateTime.UtcNow;
            hits = await PublishedLookupHitQueries.ListAsync(_db, criteria, moment, null, cancellationToken);
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
        if (_cache.TryGet(resolution.Key, out var cached))
        {
            return cached;
        }

        var structures = new Dictionary<int, PublishedStructure>();
        var matches = new List<PublishedLookupMatchDto>();
        var cellCount = 0;

        foreach (var hit in resolution.Hits)
        {
            var version = resolution.Versions[hit.SheetId];
            if (!structures.TryGetValue(hit.SheetId, out var structure))
            {
                structure = await PublishedStructureQueries.ReadAsync(_db, version, cancellationToken);
                structures[hit.SheetId] = structure;
            }

            // The value is still stored for a cell in a section or column block that was later removed.
            if (!PublishedLookupAssembler.IsOnSheet(hit, structure))
            {
                continue;
            }

            var sectionIds = PublishedLookupAssembler.ScopeSections(hit, structure);
            var cells = await PublishedLookupCellQueries.ListAroundHitAsync(_db, version, hit, sectionIds, cancellationToken);
            var values = await PublishedKindValueQueries.ReadAsync(_db, cells, wholeSheet: null, cancellationToken);
            cellCount += cells.Count;

            matches.Add(PublishedLookupAssembler.Assemble(hit, version, structure, cells, values));
        }

        var answer = new PublishedLookupDto { Key = resolution.Criteria.Key, Value = resolution.Criteria.Value, Matches = matches };
        _cache.Set(resolution.Key, answer, cellCount);
        return answer;
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

        var versions = await PublishedSheetQueries.ListVersionsUpToAsync(_db, sheetIds, moment, cancellationToken);

        foreach (var version in versions.OrderBy(version => version.VersionNumber))
        {
            result[version.SheetId] = new ResolvedSheetVersion
            {
                SheetId = version.SheetId,
                SheetPublicId = publicIds[version.SheetId],
                VersionNumber = version.VersionNumber,
                PublishedAtUtc = DateTime.SpecifyKind(version.PublishedAtUtc, DateTimeKind.Utc),
            };
        }

        return result;
    }

    /// <summary>A key nobody has given to a cell is a mistake in the request, not an empty answer.</summary>
    private async Task EnsureKeyExistsAsync(string key, CancellationToken cancellationToken)
    {
        if (!await TemplateCellQueries.LookupKeyExistsAsync(_db, key, cancellationToken))
        {
            throw new InvalidRequestException($"No template cell has the lookup key \"{key}\".");
        }
    }
}
