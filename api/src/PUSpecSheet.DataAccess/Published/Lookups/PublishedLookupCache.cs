using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Common;

namespace PUSpecSheet.DataAccess.Published.Lookups;

public sealed class PublishedLookupCache : SizedMemoryCache<PublishedLookup>
{
    private const long MaximumCells = 500_000;

    public PublishedLookupCache()
        : base(MaximumCells)
    {
    }
}
