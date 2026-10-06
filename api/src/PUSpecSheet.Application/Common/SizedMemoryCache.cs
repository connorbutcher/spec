using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;

namespace PUSpecSheet.Application.Common;

/// <summary>
/// Keeps built answers in memory under a key that names exactly what they were built from, such as a sheet
/// and one of its published versions. What is published never changes, so an entry is never wrong and
/// nothing has to be cleared; entries only leave when they go unused or the cache is full. Each entry says
/// how big it is, so a few large ones can't push everything else out unnoticed.
/// </summary>
public abstract class SizedMemoryCache<TValue>(long sizeLimit) : IDisposable
    where TValue : class
{
    private static readonly TimeSpan Unused = TimeSpan.FromMinutes(30);

    private readonly MemoryCache cache = new(new MemoryCacheOptions { SizeLimit = sizeLimit });

    public bool TryGet(string key, [NotNullWhen(true)] out TValue? value)
    {
        return cache.TryGetValue(key, out value) && value is not null;
    }

    public void Set(string key, TValue value, long size)
    {
        cache.Set(key, value, new MemoryCacheEntryOptions { Size = size + 1, SlidingExpiration = Unused });
    }

    public void Dispose()
    {
        cache.Dispose();
        GC.SuppressFinalize(this);
    }
}
