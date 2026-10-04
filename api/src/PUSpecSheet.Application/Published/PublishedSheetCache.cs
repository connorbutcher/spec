using Microsoft.Extensions.Caching.Memory;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Keeps built answers in memory. A published version never changes, so an entry is never wrong and
/// nothing has to be cleared; entries only leave when they go unused or the cache is full. Size is counted
/// in cells so a few whole sheets can't push everything else out unnoticed.
/// </summary>
public sealed class PublishedSheetCache : IDisposable
{
    private const long MaximumCells = 500_000;

    private static readonly TimeSpan Unused = TimeSpan.FromMinutes(30);

    private readonly MemoryCache cache = new(new MemoryCacheOptions { SizeLimit = MaximumCells });

    public bool TryGet(string key, out PublishedSheetDto? sheet)
    {
        return cache.TryGetValue(key, out sheet);
    }

    public void Set(string key, PublishedSheetDto sheet, int cellCount)
    {
        cache.Set(key, sheet, new MemoryCacheEntryOptions { Size = cellCount + 1, SlidingExpiration = Unused });
    }

    public void Dispose()
    {
        cache.Dispose();
    }
}
