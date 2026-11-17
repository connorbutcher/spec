using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Published.Lookups;

namespace PUSpecSheet.DataAccess.Published;

public interface IPublishedLookupHitDataService
{
    Task<List<PublishedLookupHit>> ListAsync(PublishedLookupCriteria criteria, DateTime momentUtc, int? sheetId, CancellationToken cancellationToken);

    IQueryable<PublishedLookupHit> Query(PublishedLookupCriteria criteria, DateTime momentUtc, int? sheetId);
}
