using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.Common.Configuration;
using PUSpecSheet.Api.Common.Redis;
using StackExchange.Redis;

namespace PUSpecSheet.Api.DAL.Caching;

internal class SpecSheetCache : ISpecSheetCache
{
    public const string SpecSheetsCacheName = "SpecSheetsCache";
    private const int CompressionThresholdBytes = 64 * 1024;
    private const int DefaultPageSize = 2500;

    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly string _cacheKeyPrefix;
    private readonly DistributedCacheEntryOptions _cacheOptions = new() { SlidingExpiration = TimeSpan.FromDays(7) };

    public SpecSheetCache(
        IDistributedCache cache,
        IConnectionMultiplexer connectionMultiplexer,
        IConfiguration configuration)
    {
        _cache = cache;
        _connectionMultiplexer = connectionMultiplexer;
        _cacheKeyPrefix = $"{configuration.GetCacheKeyPrefix()}sheets:";
    }

    public async Task<Sheet?> GetAsync(int specSheetId, CancellationToken cancellationToken)
    {
        var cacheKey = $"{_cacheKeyPrefix}{specSheetId}";
        var payload = await _cache.GetAsync(cacheKey, cancellationToken);
        if (payload is null)
        {
            return null;
        }

        await using var payloadStream = new MemoryStream(payload);
        Stream sourceStream = payloadStream;
        if (IsGzipCompressed(payload))
        {
            sourceStream = new GZipStream(payloadStream, CompressionMode.Decompress);
        }

        await using (sourceStream)
        {
            return await JsonSerializer.DeserializeAsync<Sheet>(sourceStream, cancellationToken: cancellationToken);
        }
    }

    public async Task SetAsync(Sheet sheet, CancellationToken cancellationToken)
    {
        if (sheet.Version?.SourceSpecSheetId is not { } specSheetId)
        {
            return;
        }

        var cacheKey = $"{_cacheKeyPrefix}{specSheetId}";
        var existingPayload = await _cache.GetAsync(cacheKey, cancellationToken);
        if (existingPayload is not null)
        {
            return;
        }

        var payload = JsonSerializer.SerializeToUtf8Bytes(sheet);
        if (payload.Length >= CompressionThresholdBytes)
        {
            payload = Compress(payload);
        }

        await _cache.SetAsync(cacheKey, payload, _cacheOptions, cancellationToken);
    }

    public async Task RemoveAsync(int specSheetId, CancellationToken cancellationToken)
    {
        var cacheKey = $"{_cacheKeyPrefix}{specSheetId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);
    }

    public async IAsyncEnumerable<string> GetAllKeysAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var searchPrefix = _cacheKeyPrefix;
        var seenKeys = new HashSet<string>();
        var redisKeyEnumerator = new RedisKeyEnumerator(_connectionMultiplexer);
        await foreach (var key in redisKeyEnumerator.GetKeysAsync($"{searchPrefix}*", DefaultPageSize).WithCancellation(cancellationToken))
        {
            if (seenKeys.Add(key))
            {
                yield return key;
            }
        }
    }

    private static bool IsGzipCompressed(byte[] payload)
    {
        return payload.Length >= 2 && payload[0] == 0x1F && payload[1] == 0x8B;
    }

    private static byte[] Compress(byte[] payload)
    {
        using var compressedStream = new MemoryStream();
        using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Fastest, leaveOpen: true))
        {
            gzipStream.Write(payload);
        }

        return compressedStream.ToArray();
    }
}
