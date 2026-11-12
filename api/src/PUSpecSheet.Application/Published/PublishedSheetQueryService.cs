using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence.Published;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Published;

public sealed class PublishedSheetQueryService(
    IPublishedSheetRepository sheets,
    PublishedVersionResolver resolver,
    IPublishedStructureRepository structureReader,
    IPublishedCellRepository cellReader,
    IPublishedValueRepository valueReader,
    PublishedSheetCache cache) : IPublishedSheetQueryService
{
    public async Task<PublishedSheetReferenceDto> FindAsync(string phaseCode, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await sheets.FindReferenceAsync(phaseCode, sheetTypeId, cancellationToken)
            ?? throw new NotFoundException($"Phase {phaseCode} has no sheet of type {sheetTypeId}.");
    }

    public async Task<IReadOnlyList<PublishedVersionDto>> VersionsAsync(Guid sheetPublicId, CancellationToken cancellationToken)
    {
        var versions = await sheets.ListVersionsAsync(sheetPublicId, cancellationToken);

        if (versions.Count == 0
            && !await sheets.ExistsAsync(sheetPublicId, cancellationToken))
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

        var structure = await structureReader.ReadAsync(version, cancellationToken);
        var scope = selection.IsEverything ? null : structure.ScopeFor(selection);
        var cells = await cellReader.ReadAsync(version, scope, selection.IncludeLabels, cancellationToken);
        var values = await valueReader.ReadAsync(cells, selection.IsEverything ? version : null, cancellationToken);

        var sheet = PublishedSheetAssembler.Assemble(version, selection, structure, cells, values);
        cache.Set(key, sheet, cells.Count);
        return sheet;
    }
}
