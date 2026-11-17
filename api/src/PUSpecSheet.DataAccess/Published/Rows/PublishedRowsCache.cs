using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Common;

namespace PUSpecSheet.DataAccess.Published.Rows;

public sealed class PublishedRowsCache : SizedMemoryCache<PublishedRows>
{
    private const long MaximumCells = 500_000;

    public PublishedRowsCache()
        : base(MaximumCells)
    {
    }
}
