using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.DataAccess.SheetTypes;

namespace PUSpecSheet.DataAccess.Published.Sheets;

public sealed class PublishedSheetsService : IPublishedSheetsService
{
    private readonly PublishedVersionResolver _resolver;
    private readonly PublishedSheetCache _cache;
    private readonly IPublishedCellDataService _publishedCellData;
    private readonly IPublishedValueDataService _publishedValueData;
    private readonly IPublishedSheetDataService _publishedSheetData;
    private readonly IPublishedStructureDataService _publishedStructureData;
    private readonly ISheetTypeDataService _sheetTypes;

    public PublishedSheetsService(
        PublishedVersionResolver resolver,
        PublishedSheetCache cache,
        IPublishedCellDataService publishedCellData,
        IPublishedValueDataService publishedValueData,
        IPublishedSheetDataService publishedSheetData,
        IPublishedStructureDataService publishedStructureData,
        ISheetTypeDataService sheetTypes)
    {
        _resolver = resolver;
        _cache = cache;
        _publishedCellData = publishedCellData;
        _publishedValueData = publishedValueData;
        _publishedSheetData = publishedSheetData;
        _publishedStructureData = publishedStructureData;
        _sheetTypes = sheetTypes;
    }

    public async Task<PublishedSheetReference> FindAsync(string phaseCode, int sheetTypeId, IReadOnlyDictionary<string, string> identifiers, CancellationToken cancellationToken)
    {
        var found = await ListAsync(phaseCode, sheetTypeId, identifiers, cancellationToken);
        if (found.Count == 0)
        {
            throw new NotFoundException(identifiers.Count == 0
                ? $"Phase {phaseCode} has no sheet of type {sheetTypeId}."
                : $"Phase {phaseCode} has no sheet of type {sheetTypeId} with those identifiers.");
        }

        if (found.Count > 1)
        {
            var keys = string.Join(", ", found.SelectMany(sheet => sheet.Identifiers.Keys).Distinct(StringComparer.OrdinalIgnoreCase));
            throw new ConflictException($"{found.Count} sheets of type {sheetTypeId} in phase {phaseCode} match. Narrow them down by identifier: {keys}.");
        }

        return found[0];
    }

    public async Task<IReadOnlyList<PublishedSheetReference>> ListAsync(string phaseCode, int sheetTypeId, IReadOnlyDictionary<string, string> identifiers, CancellationToken cancellationToken)
    {
        List<PublishedIdentifierFilter> filters = identifiers.Count == 0
            ? []
            : PublishedIdentifierFilters.For(sheetTypeId, await _sheetTypes.ListIdentifiersAsync(sheetTypeId, cancellationToken), identifiers);
        return await _publishedSheetData.ListReferencesAsync(phaseCode, sheetTypeId, filters, cancellationToken);
    }

    public async Task<IReadOnlyList<PublishedVersion>> VersionsAsync(Guid sheetPublicId, CancellationToken cancellationToken)
    {
        var versions = await _publishedSheetData.ListVersionsAsync(sheetPublicId, cancellationToken);

        if (versions.Count == 0
            && !await _publishedSheetData.ExistsAsync(sheetPublicId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetPublicId} was not found.");
        }

        return versions;
    }

    public Task<ResolvedSheetVersion> ResolveAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken)
    {
        return _resolver.ResolveAsync(sheetPublicId, point, cancellationToken);
    }

    public async Task<PublishedSheet> ReadAsync(ResolvedSheetVersion version, PublishedSheetSelection selection, CancellationToken cancellationToken)
    {
        var key = version.KeyFor(selection);
        if (_cache.TryGet(key, out var cached))
        {
            return cached;
        }

        var structure = await _publishedStructureData.ReadAsync(version, cancellationToken);
        var scope = selection.IsEverything ? null : structure.ScopeFor(selection);
        var cells = await _publishedCellData.ReadAsync(version, scope, selection.IncludeLabels, cancellationToken);
        var values = await _publishedValueData.ReadAsync(cells, selection.IsEverything ? version : null, cancellationToken);

        var sheet = PublishedSheetAssembler.Assemble(version, selection, structure, cells, values);
        _cache.Set(key, sheet, cells.Count);
        return sheet;
    }
}
