using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Published.Queries;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Published.Sheets;

public sealed class PublishedSheetsService : IPublishedSheetsService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly PublishedVersionResolver _resolver;
    private readonly PublishedSheetCache _cache;

    public PublishedSheetsService(
        PuSpecSheetDbContext db,
        PublishedVersionResolver resolver,
        PublishedSheetCache cache)
    {
        _db = db;
        _resolver = resolver;
        _cache = cache;
    }

    public async Task<PublishedSheetReferenceDto> FindAsync(string phaseCode, int sheetTypeId, IReadOnlyDictionary<string, string> identifiers, CancellationToken cancellationToken)
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

    public async Task<IReadOnlyList<PublishedSheetReferenceDto>> ListAsync(string phaseCode, int sheetTypeId, IReadOnlyDictionary<string, string> identifiers, CancellationToken cancellationToken)
    {
        var filters = await PublishedIdentifierFilters.ResolveAsync(_db, sheetTypeId, identifiers, cancellationToken);
        return await PublishedSheetQueries.ListReferencesAsync(_db, phaseCode, sheetTypeId, filters, cancellationToken);
    }

    public async Task<IReadOnlyList<PublishedVersionDto>> VersionsAsync(Guid sheetPublicId, CancellationToken cancellationToken)
    {
        var versions = await PublishedSheetQueries.ListVersionsAsync(_db, sheetPublicId, cancellationToken);

        if (versions.Count == 0
            && !await PublishedSheetQueries.ExistsAsync(_db, sheetPublicId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetPublicId} was not found.");
        }

        return versions;
    }

    public Task<ResolvedSheetVersion> ResolveAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken)
    {
        return _resolver.ResolveAsync(sheetPublicId, point, cancellationToken);
    }

    public async Task<PublishedSheetDto> ReadAsync(ResolvedSheetVersion version, PublishedSheetSelection selection, CancellationToken cancellationToken)
    {
        var key = version.KeyFor(selection);
        if (_cache.TryGet(key, out var cached))
        {
            return cached;
        }

        var structure = await PublishedStructureQueries.ReadAsync(_db, version, cancellationToken);
        var scope = selection.IsEverything ? null : structure.ScopeFor(selection);
        var cells = await PublishedCellQueries.ReadAsync(_db, version, scope, selection.IncludeLabels, cancellationToken);
        var values = await PublishedValueQueries.ReadAsync(_db, cells, selection.IsEverything ? version : null, cancellationToken);

        var sheet = PublishedSheetAssembler.Assemble(version, selection, structure, cells, values);
        _cache.Set(key, sheet, cells.Count);
        return sheet;
    }
}
