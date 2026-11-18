using PUSpecSheet.Api.Common.Configuration;
using PUSpecSheet.Api.Common.Redis;
using PUSpecSheet.CheckInOut.Api.Models;

namespace PUSpecSheet.CheckInOut.Api.Services.Presence;

///<inheritdoc />
public class SheetPresenceService : ISheetPresenceService
{
    public const string SheetPresenceCacheName = "SheetPresenceCache";

    private readonly ICacheProvider<ConnectedUser> _cache;
    private readonly string _cacheKeyPrefix;

    public SheetPresenceService(
        [FromKeyedServices(SheetPresenceCacheName)] ICacheProvider<ConnectedUser> cache,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(cache, nameof(cache));

        _cache = cache;
        _cacheKeyPrefix = $"{configuration.GetCacheKeyPrefix()}presence:";
    }

    ///<inheritdoc />
    public async Task JoinAsync(int sheetId, ConnectedUser user)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sheetId, 0);
        ArgumentNullException.ThrowIfNull(user);

        var cacheKey = CacheKey(sheetId, user.ConnectionId);
        if (!await _cache.TrySetAsync(cacheKey, user))
        {
            await _cache.TryUpdateAsync(cacheKey, user);
        }
    }

    ///<inheritdoc />
    public async Task LeaveAsync(int sheetId, string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        await _cache.RemoveAsync(CacheKey(sheetId, connectionId));
    }

    ///<inheritdoc />
    public async Task<IReadOnlyList<int>> LeaveAllAsync(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var sheetIds = new List<int>();
        var keys = await _cache.GetAllKeysAsync(_cacheKeyPrefix).ToListAsync();
        foreach (var key in keys.Where(k => k.EndsWith($":{connectionId}", StringComparison.Ordinal)))
        {
            var parts = key[_cacheKeyPrefix.Length..].Split(':', 2);
            if (parts.Length == 2 && int.TryParse(parts[0], out var sheetId))
            {
                sheetIds.Add(sheetId);
            }

            await _cache.RemoveAsync(key);
        }

        return sheetIds;
    }

    ///<inheritdoc />
    public async Task<SheetPresence> GetPresenceAsync(int sheetId)
    {
        var users = await _cache.GetAllValuesAsync($"{_cacheKeyPrefix}{sheetId}:")
            .OfType<ConnectedUser>()
            .ToListAsync();

        return new SheetPresence { SheetId = sheetId, Users = users };
    }

    private string CacheKey(int sheetId, string connectionId)
    {
        return $"{_cacheKeyPrefix}{sheetId}:{connectionId}";
    }
}
