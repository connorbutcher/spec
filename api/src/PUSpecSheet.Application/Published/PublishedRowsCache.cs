using Microsoft.Extensions.Caching.Memory;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Keeps built rows answers in memory, like <see cref="PublishedSheetCache"/>. A published version never
/// changes and neither does what its columns are called, so an entry is never wrong; entries only leave
/// when they go unused or the cache is full. Size is counted in cells.
/// </summary>
public sealed class PublishedRowsCache : IDisposable
{
    private const long MaximumCells = 500_000;

    private static readonly TimeSpan Unused = TimeSpan.FromMinutes(30);

    private readonly MemoryCache cache = new(new MemoryCacheOptions { SizeLimit = MaximumCells });

    public bool TryGet(string key, out PublishedRowsDto? rows)
    {
        return cache.TryGetValue(key, out rows);
    }

    public void Set(string key, PublishedRowsDto rows, int cellCount)
    {
        cache.Set(key, rows, new MemoryCacheEntryOptions { Size = cellCount + 1, SlidingExpiration = Unused });
    }

    public void Dispose()
    {
        cache.Dispose();
    }
}
