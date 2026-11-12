using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Published.Lookups;

/// <summary>
/// Keeps built lookup answers in memory under <see cref="PublishedLookupResolution.Key"/>, the same key the
/// answer's ETag is made from. The key names the cells found and the version each sheet is read at, so an
/// equal key means an equal answer and an entry is never wrong. Size is counted in cells.
/// </summary>
public sealed class PublishedLookupCache : SizedMemoryCache<PublishedLookupDto>
{
    private const long MaximumCells = 500_000;

    public PublishedLookupCache()
        : base(MaximumCells)
    {
    }
}
