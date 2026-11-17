using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Common;

namespace PUSpecSheet.DataAccess.Published.Sheets;

// A published version never changes, so an entry is never wrong. Size is counted in cells.
public sealed class PublishedSheetCache : SizedMemoryCache<PublishedSheet>
{
    private const long MaximumCells = 500_000;

    public PublishedSheetCache()
        : base(MaximumCells)
    {
    }
}
