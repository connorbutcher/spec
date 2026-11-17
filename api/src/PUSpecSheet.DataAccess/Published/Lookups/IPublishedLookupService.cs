using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Published.Lookups;

public interface IPublishedLookupService
{
    // Two small queries, done first, so an unchanged answer is recognised before any data is read.
    Task<PublishedLookupResolution> ResolveAsync(
        PublishedLookupCriteria criteria,
        Guid? sheetPublicId,
        PublishedVersionPoint point,
        CancellationToken cancellationToken);

    Task<PublishedLookup> ReadAsync(PublishedLookupResolution resolution, CancellationToken cancellationToken);
}
