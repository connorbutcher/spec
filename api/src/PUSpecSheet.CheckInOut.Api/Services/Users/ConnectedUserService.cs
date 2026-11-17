using PUSpecSheet.Api.Common.Configuration;
using PUSpecSheet.Api.Common.Redis;
using PUSpecSheet.CheckInOut.Api.Models;

namespace PUSpecSheet.CheckInOut.Api.Services.Users;

///<inheritdoc />
public class ConnectedUserService : IConnectedUserService
{
    public const string ConnectedUsersCacheName = "ConnectedUsersCache";

    private static readonly IReadOnlyList<string> _userColours =
    [
        "#00AA55",
        "#009FD4",
        "#B381B3",
        "#939393",
        "#E3BC00",
        "#D47500",
        "#DC2A2A",
    ];
    private readonly ICacheProvider<ConnectedUser> _cache;
    private readonly ILogger<IConnectedUserService> _logger;
    private readonly string _cacheKeyPrefix;

    /// <summary>
    /// Initializes an instance of <see cref="ConnectedUserService"/>.
    /// </summary>
    /// <param name="logger">An instance of the logger.</param>
    public ConnectedUserService(
        [FromKeyedServices(ConnectedUsersCacheName)] ICacheProvider<ConnectedUser> cache,
        ILogger<IConnectedUserService> logger,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(cache, nameof(cache));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _cache = cache;
        _logger = logger;
        _cacheKeyPrefix = $"{configuration.GetCacheKeyPrefix()}connectedUsers:";
    }

    ///<inheritdoc />
    public async Task<IReadOnlyList<ConnectedUser>> GetAllConnectedUsersAsync(CancellationToken cancellationToken = default)
    {
        var values = await _cache.GetAllValuesAsync(_cacheKeyPrefix, cancellationToken)
            .OfType<ConnectedUser>()
            .ToListAsync(cancellationToken: cancellationToken);

        return values.AsReadOnly();
    }

    ///<inheritdoc />
    public async Task<bool> TryAddUserAsync(string connectionId, string username, string uid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId, nameof(connectionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(username, nameof(username));
        ArgumentException.ThrowIfNullOrWhiteSpace(uid, nameof(uid));

        var currentUserCount = await _cache.GetKeyCountAsync(_cacheKeyPrefix);
        var nextColourIndex = (int)(currentUserCount % _userColours.Count);
        var colour = _userColours[nextColourIndex];

        var cacheKey = $"{_cacheKeyPrefix}{connectionId}";

        if (await _cache.TrySetAsync(cacheKey, new ConnectedUser(connectionId, username, uid, colour)))
        {
            _logger.LogInformation("Adding connection - {ConnectionId}. Currently {UsersCount} users connected.", connectionId, currentUserCount + 1);
            return true;
        }

        _logger.LogError("Unable to add connection - {ConnectionId}", connectionId);
        return false;
    }

    ///<inheritdoc />
    public async Task<ConnectedUser?> GetConnectedUserAsync(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId, nameof(connectionId));

        var cacheKey = $"{_cacheKeyPrefix}{connectionId}";
        var clientContext = await _cache.GetAsync(cacheKey);
        if (clientContext != null)
        {
            _logger.LogInformation("Successfully retrieved connected user - {ConnectionId}", connectionId);
            return clientContext;
        }

        _logger.LogWarning("Unable to find connected user - {ConnectionId}", connectionId);
        return null;
    }

    ///<inheritdoc />
    public async Task RemoveConnectedUserAsync(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId, nameof(connectionId));

        var cacheKey = $"{_cacheKeyPrefix}{connectionId}";
        await _cache.RemoveAsync(cacheKey);

        _logger.LogInformation("Removed connected user - {ConnectionId}", connectionId);
    }
}
