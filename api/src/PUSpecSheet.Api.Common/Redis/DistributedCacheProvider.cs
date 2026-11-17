using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace PUSpecSheet.Api.Common.Redis;

public sealed class DistributedCacheProvider<TValue> : ICacheProvider<TValue>
{
    private const int DefaultPageSize = 2500;

    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly DistributedCacheEntryOptions _cacheOptions = new() { SlidingExpiration = TimeSpan.FromDays(7) };

    public DistributedCacheProvider(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
    {
        _cache = cache;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async IAsyncEnumerable<string> GetAllKeysAsync(string? searchPrefix, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var searchPattern = string.IsNullOrEmpty(searchPrefix) ? "*" : $"{searchPrefix}*";
        var seenKeys = new HashSet<string>();

        var redisKeyEnumerator = new RedisKeyEnumerator(_connectionMultiplexer);
        await foreach (var key in redisKeyEnumerator.GetKeysAsync(searchPattern, DefaultPageSize).WithCancellation(cancellationToken))
        {
            if (seenKeys.Add(key))
            {
                yield return key;
            }
        }
    }

    public async Task<long> GetKeyCountAsync(string? searchPrefix, CancellationToken cancellationToken = default)
    {
        long count = 0;
        await foreach (var _ in GetAllKeysAsync(searchPrefix, cancellationToken))
        {
            count++;
        }
        return count;
    }

    public async IAsyncEnumerable<TValue> GetAllValuesAsync(string? searchPrefix, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var key in GetAllKeysAsync(searchPrefix, cancellationToken))
        {
            var json = await _cache.GetStringAsync(key, cancellationToken);
            if (json is null)
            {
                continue;
            }

            var val = JsonSerializer.Deserialize<TValue>(json);
            if (val is not null)
            {
                yield return val;
            }
        }
    }

    public async Task<TValue?> GetAsync(string key, CancellationToken token = default)
    {
        var json = await _cache.GetStringAsync(key, token);
        return json is null ? default : JsonSerializer.Deserialize<TValue>(json);
    }

    public async Task<bool> TrySetAsync(string key, TValue value, CancellationToken token = default)
    {
        var existingValue = await _cache.GetStringAsync(key, token);
        if (existingValue is null)
        {
            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, _cacheOptions, token);
            return true;
        }

        return false;
    }

    public async Task<bool> TryUpdateAsync(string key, TValue value, CancellationToken token = default)
    {
        var existingValue = await _cache.GetStringAsync(key, token);
        if (existingValue is not null)
        {
            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, _cacheOptions, token);
            return true;
        }

        return false;
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        return _cache.RemoveAsync(key, token);
    }
}
