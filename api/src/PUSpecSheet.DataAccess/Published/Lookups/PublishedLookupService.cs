using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.DataAccess.Templates;

namespace PUSpecSheet.DataAccess.Published.Lookups;

public sealed class PublishedLookupService : IPublishedLookupService
{
    private readonly PublishedVersionResolver _resolver;
    private readonly PublishedLookupCache _cache;
    private readonly ITemplateCellDataService _templateCellData;
    private readonly IPublishedValueDataService _publishedValueData;
    private readonly IPublishedLookupCellDataService _publishedLookupCellData;
    private readonly IPublishedLookupHitDataService _publishedLookupHitData;
    private readonly IPublishedSheetDataService _publishedSheetData;
    private readonly IPublishedStructureDataService _publishedStructureData;

    public PublishedLookupService(
        PublishedVersionResolver resolver,
        PublishedLookupCache cache,
        ITemplateCellDataService templateCellData,
        IPublishedValueDataService publishedValueData,
        IPublishedLookupCellDataService publishedLookupCellData,
        IPublishedLookupHitDataService publishedLookupHitData,
        IPublishedSheetDataService publishedSheetData,
        IPublishedStructureDataService publishedStructureData)
    {
        _resolver = resolver;
        _cache = cache;
        _templateCellData = templateCellData;
        _publishedValueData = publishedValueData;
        _publishedLookupCellData = publishedLookupCellData;
        _publishedLookupHitData = publishedLookupHitData;
        _publishedSheetData = publishedSheetData;
        _publishedStructureData = publishedStructureData;
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
            hits = await _publishedLookupHitData.ListAsync(criteria, version.PublishedAtUtc, version.SheetId, cancellationToken);
            versions = new Dictionary<int, ResolvedSheetVersion> { [version.SheetId] = version };
        }
        else
        {
            var moment = point.AtUtc ?? DateTime.UtcNow;
            hits = await _publishedLookupHitData.ListAsync(criteria, moment, null, cancellationToken);
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

    public async Task<PublishedLookup> ReadAsync(PublishedLookupResolution resolution, CancellationToken cancellationToken)
    {
        if (_cache.TryGet(resolution.Key, out var cached))
        {
            return cached;
        }

        var structures = new Dictionary<int, PublishedStructure>();
        var matches = new List<PublishedLookupMatch>();
        var cellCount = 0;

        foreach (var hit in resolution.Hits)
        {
            var version = resolution.Versions[hit.SheetId];
            if (!structures.TryGetValue(hit.SheetId, out var structure))
            {
                structure = await _publishedStructureData.ReadAsync(version, cancellationToken);
                structures[hit.SheetId] = structure;
            }

            // The value is still stored for a cell in a section or column block that was later removed.
            if (!PublishedLookupAssembler.IsOnSheet(hit, structure))
            {
                continue;
            }

            var sectionIds = PublishedLookupAssembler.ScopeSections(hit, structure);
            var cells = await _publishedLookupCellData.ListAroundHitAsync(version, hit, sectionIds, cancellationToken);
            var values = await _publishedValueData.ReadAsync(cells, wholeSheet: null, cancellationToken);
            cellCount += cells.Count;

            matches.Add(PublishedLookupAssembler.Assemble(hit, version, structure, cells, values));
        }

        var answer = new PublishedLookup { Key = resolution.Criteria.Key, Value = resolution.Criteria.Value, Matches = matches };
        _cache.Set(resolution.Key, answer, cellCount);
        return answer;
    }

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

        var versions = await _publishedSheetData.ListVersionsUpToAsync(sheetIds, moment, cancellationToken);

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

    // A key nobody has given to a cell is a mistake in the request, not an empty answer.
    private async Task EnsureKeyExistsAsync(string key, CancellationToken cancellationToken)
    {
        if (!await _templateCellData.LookupKeyExistsAsync(key, cancellationToken))
        {
            throw new InvalidRequestException($"No template cell has the lookup key \"{key}\".");
        }
    }
}
