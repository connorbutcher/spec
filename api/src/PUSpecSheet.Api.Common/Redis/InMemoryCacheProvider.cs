using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace PUSpecSheet.Api.Common.Redis;

/// <summary>
/// A cache held in this process, standing in for <see cref="DistributedCacheProvider{TValue}"/> while Redis is
/// disabled for local running. Values are stored as JSON like the Redis cache, but are lost on restart and not shared
/// between instances.
/// </summary>
public sealed class InMemoryCacheProvider<TValue> : ICacheProvider<TValue>
{
    private readonly ConcurrentDictionary<string, string> _values = new();

    public async IAsyncEnumerable<string> GetAllKeysAsync(string? searchPrefix, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        foreach (var key in _values.Keys)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(searchPrefix) || key.StartsWith(searchPrefix, StringComparison.Ordinal))
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
            if (!_values.TryGetValue(key, out var json))
            {
                continue;
            }

            var value = JsonSerializer.Deserialize<TValue>(json);
            if (value is not null)
            {
                yield return value;
            }
        }
    }

    public Task<TValue?> GetAsync(string key, CancellationToken token = default)
    {
        var value = _values.TryGetValue(key, out var json) ? JsonSerializer.Deserialize<TValue>(json) : default;
        return Task.FromResult(value);
    }

    public Task<bool> TrySetAsync(string key, TValue value, CancellationToken token = default)
    {
        return Task.FromResult(_values.TryAdd(key, JsonSerializer.Serialize(value)));
    }

    public Task<bool> TryUpdateAsync(string key, TValue value, CancellationToken token = default)
    {
        if (!_values.ContainsKey(key))
        {
            return Task.FromResult(false);
        }

        _values[key] = JsonSerializer.Serialize(value);
        return Task.FromResult(true);
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        _values.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
