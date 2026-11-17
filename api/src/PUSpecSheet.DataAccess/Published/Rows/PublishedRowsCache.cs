using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Published.Sheets;

namespace PUSpecSheet.DataAccess.Published.Rows;

/// <summary>
/// Keeps built rows answers in memory, like <see cref="PublishedSheetCache"/>. A published version never
/// changes and neither does what its columns are called, so an entry is never wrong. Size is counted in cells.
/// </summary>
public sealed class PublishedRowsCache : SizedMemoryCache<PublishedRows>
{
    private const long MaximumCells = 500_000;

    public PublishedRowsCache()
        : base(MaximumCells)
    {
    }
}
