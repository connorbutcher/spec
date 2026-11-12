using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Published.Queries;
using PUSpecSheet.Application.Published.Versions;
using PUSpecSheet.Contracts.Published;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Published.Sheets;

public sealed class PublishedSheetsService : IPublishedSheetsService
{
    private readonly PuSpecSheetDbContext db;
    private readonly PublishedVersionResolver resolver;
    private readonly PublishedSheetCache cache;

    public PublishedSheetsService(
        PuSpecSheetDbContext db,
        PublishedVersionResolver resolver,
        PublishedSheetCache cache)
    {
        this.db = db;
        this.resolver = resolver;
        this.cache = cache;
    }

    public async Task<PublishedSheetReferenceDto> FindAsync(string phaseCode, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await PublishedSheetQueries.FindReferenceAsync(db, phaseCode, sheetTypeId, cancellationToken)
            ?? throw new NotFoundException($"Phase {phaseCode} has no sheet of type {sheetTypeId}.");
    }

    public async Task<IReadOnlyList<PublishedVersionDto>> VersionsAsync(Guid sheetPublicId, CancellationToken cancellationToken)
    {
        var versions = await PublishedSheetQueries.ListVersionsAsync(db, sheetPublicId, cancellationToken);

        if (versions.Count == 0
            && !await PublishedSheetQueries.ExistsAsync(db, sheetPublicId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetPublicId} was not found.");
        }

        return versions;
    }

    public Task<ResolvedSheetVersion> ResolveAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken)
    {
        return resolver.ResolveAsync(sheetPublicId, point, cancellationToken);
    }

    public async Task<PublishedSheetDto> ReadAsync(ResolvedSheetVersion version, PublishedSheetSelection selection, CancellationToken cancellationToken)
    {
        var key = version.KeyFor(selection);
        if (cache.TryGet(key, out var cached))
        {
            return cached;
        }

        var structure = await PublishedStructureQueries.ReadAsync(db, version, cancellationToken);
        var scope = selection.IsEverything ? null : structure.ScopeFor(selection);
        var cells = await PublishedCellQueries.ReadAsync(db, version, scope, selection.IncludeLabels, cancellationToken);
        var values = await PublishedValueQueries.ReadAsync(db, cells, selection.IsEverything ? version : null, cancellationToken);

        var sheet = PublishedSheetAssembler.Assemble(version, selection, structure, cells, values);
        cache.Set(key, sheet, cells.Count);
        return sheet;
    }
}
