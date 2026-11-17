using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Published.Lookups;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Published;

public interface IPublishedLookupCellDataService
{
    Task<List<PublishedLookupCellRecord>> ListAroundHitAsync(
        ResolvedSheetVersion version,
        PublishedLookupHit hit,
        IReadOnlyList<int>? sectionIds,
        CancellationToken cancellationToken);

    Task<List<PublishedLookupCellRecord>> ListOnSheetAsync(ResolvedSheetVersion version, CancellationToken cancellationToken);

    Task<List<PublishedLookupCellRecord>> ListInRowsAsync(ResolvedSheetVersion version, IReadOnlyList<Guid> rowPublicIds, CancellationToken cancellationToken);

    Task<List<PublishedLookupCellRecord>> ListHeadersAsync(ResolvedSheetVersion version, IReadOnlyList<int> tableIds, CancellationToken cancellationToken);

    IQueryable<PublishedLookupCellRecord> Query(
        ResolvedSheetVersion version,
        PublishedLookupHit hit,
        IReadOnlyList<int>? sectionIds);

    IQueryable<PublishedLookupCellRecord> QuerySheet(ResolvedSheetVersion version);

    IQueryable<PublishedLookupCellRecord> QueryRows(ResolvedSheetVersion version, IReadOnlyList<Guid> rowPublicIds);

    IQueryable<PublishedLookupCellRecord> QueryHeaders(ResolvedSheetVersion version, IReadOnlyList<int> tableIds);
}
