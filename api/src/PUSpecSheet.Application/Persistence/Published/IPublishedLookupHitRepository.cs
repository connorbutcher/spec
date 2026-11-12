using PUSpecSheet.Application.Published;

namespace PUSpecSheet.Application.Persistence.Published;

/// <summary>The cells whose published value answers a lookup.</summary>
public interface IPublishedLookupHitRepository
{
    /// <summary>The cells holding the value under the key at the moment, on one sheet or on every sheet.</summary>
    Task<List<PublishedLookupHit>> ListAsync(PublishedLookupCriteria criteria, DateTime momentUtc, int? sheetId, CancellationToken cancellationToken);
}
