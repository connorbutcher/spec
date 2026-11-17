using StackExchange.Redis;

namespace PUSpecSheet.Api.Common.Redis;

public sealed class RedisKeyEnumerator
{
    private readonly IConnectionMultiplexer _redis;

    public RedisKeyEnumerator(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    /// <summary>
    /// Enumerates all keys in the Redis cache with the search pattern, fetching all pages of results.
    /// This uses SCAN internally and does not guarantee that keys returned are unique!
    /// </summary>
    public async IAsyncEnumerable<string> GetKeysAsync(string searchPattern, int pageSize = 250)
    {
        var server = GetServer();
        long cursor = 0;
        var moreData = true;

        while (moreData)
        {
            var keysAsyncEnumerable = server.KeysAsync(pattern: searchPattern, pageSize: pageSize, cursor: cursor);
            var foundAnyKeys = false;

            await foreach (var key in keysAsyncEnumerable)
            {
                foundAnyKeys = true;
                var keyString = key.ToString();
                if (!string.IsNullOrEmpty(keyString))
                {
                    yield return keyString;
                }
            }

            cursor = ((IScanningCursor)keysAsyncEnumerable).Cursor;

            // If the cursor is 0, we've reached the end of the scan
            // or if we didn't find any keys, we can also stop
            moreData = cursor != 0 && foundAnyKeys;
        }
    }

    private IServer GetServer()
    {
        var endpoint = _redis.GetEndPoints().First();
        return _redis.GetServer(endpoint);
    }
}
