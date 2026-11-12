using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Published.Sheets;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Published.Rows;

/// <summary>
/// Keeps built rows answers in memory, like <see cref="PublishedSheetCache"/>. A published version never
/// changes and neither does what its columns are called, so an entry is never wrong. Size is counted in cells.
/// </summary>
public sealed class PublishedRowsCache() : SizedMemoryCache<PublishedRowsDto>(MaximumCells)
{
    private const long MaximumCells = 500_000;
}
