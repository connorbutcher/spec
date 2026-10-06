using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Keeps built published-sheet answers in memory, keyed by sheet, version and what was asked for. A
/// published version never changes, so an entry is never wrong. Size is counted in cells.
/// </summary>
public sealed class PublishedSheetCache() : SizedMemoryCache<PublishedSheetDto>(MaximumCells)
{
    private const long MaximumCells = 500_000;
}
