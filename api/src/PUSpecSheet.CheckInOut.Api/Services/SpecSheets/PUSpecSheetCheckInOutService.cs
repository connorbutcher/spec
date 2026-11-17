using PUSpecSheet.Api.Common.Configuration;
using PUSpecSheet.Api.Common.Redis;
using PUSpecSheet.CheckInOut.Api.Models;

namespace PUSpecSheet.CheckInOut.Api.Services.SpecSheets;

///<inheritdoc />
public class PUSpecSheetCheckInOutService : IPUSpecSheetCheckInOutService
{
    public const string PUSpecSheetCheckedOutCacheName = "PUSpecSheetCheckOutCache";
    public const string PUSpecSheetCheckoutRequestsCacheName = "PUSpecSheetCheckoutRequestsCache";

    private readonly ICacheProvider<ConnectedUser> _checkedOutPUSpecSheetsCache;
    private readonly ICacheProvider<CheckoutRequest> _checkOutRequestsCache;

    private readonly string _checkoutCacheKey;
    private readonly string _checkoutRequestCacheKey;

    private const string _checkedOutCacheKeyPrefix = "checkedout";
    private const string _checkoutRequestCacheKeyPrefix = "checkoutrequests";

    public PUSpecSheetCheckInOutService(
        [FromKeyedServices(PUSpecSheetCheckedOutCacheName)] ICacheProvider<ConnectedUser> checkedOutTemplatesCache,
        [FromKeyedServices(PUSpecSheetCheckoutRequestsCacheName)] ICacheProvider<CheckoutRequest> checkOutRequestsCache,
        IConfiguration configuration)
    {
        var cacheKeyPrefix = configuration.GetCacheKeyPrefix();

        _checkoutCacheKey = $"{cacheKeyPrefix}{_checkedOutCacheKeyPrefix}";
        _checkoutRequestCacheKey = $"{cacheKeyPrefix}{_checkoutRequestCacheKeyPrefix}";

        _checkedOutPUSpecSheetsCache = checkedOutTemplatesCache;
        _checkOutRequestsCache = checkOutRequestsCache;
    }

    #region Check Out

    ///<inheritdoc />
    public async Task<bool> TryCheckoutPUSpecSheetAsync(int puSpecSheetId, ConnectedUser connectedUser)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(puSpecSheetId, 0);
        ArgumentNullException.ThrowIfNull(connectedUser);

        var cacheKey = $"{_checkoutCacheKey}:{puSpecSheetId}";
        var checkedOutBy = await _checkedOutPUSpecSheetsCache.GetAsync(cacheKey);
        if (checkedOutBy != null)
        {
            return false;
        }

        return await _checkedOutPUSpecSheetsCache.TrySetAsync(cacheKey, connectedUser);
    }

    #endregion Check Out

    #region Check In

    ///<inheritdoc />
    public async Task<bool> TryCheckInPUSpecSheetAsync(int puSpecSheetId, string connectionId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(puSpecSheetId, 0);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var cacheKey = $"{_checkoutCacheKey}:{puSpecSheetId}";

        var checkedOutBy = await _checkedOutPUSpecSheetsCache.GetAsync(cacheKey);
        if (checkedOutBy == null || checkedOutBy.ConnectionId != connectionId)
        {
            return false;
        }

        await _checkedOutPUSpecSheetsCache.RemoveAsync(cacheKey);
        return true;
    }

    ///<inheritdoc />
    public async Task<bool> TryRemoveAllCheckInsForUserAsync(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var allKeys = await _checkedOutPUSpecSheetsCache.GetAllKeysAsync(_checkoutCacheKey).ToListAsync();
        foreach (var key in allKeys)
        {
            var value = await _checkedOutPUSpecSheetsCache.GetAsync(key);
            if (value?.ConnectionId == connectionId)
            {
                await _checkedOutPUSpecSheetsCache.RemoveAsync(key);
            }
        }

        return true;
    }

    #endregion Check In

    #region Request Checkouts

    ///<inheritdoc />
    public async Task<CheckoutRequest?> RequestCheckoutAsync(int id, ConnectedUser requestingUser)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(id, 0);
        ArgumentNullException.ThrowIfNull(requestingUser);

        // Get the responding user
        var respondingCheckout = await GetCheckedOutUserByIdAsync(id);
        if (respondingCheckout == null)
        {
            return null;
        }

        var checkoutRequestContextId = Guid.NewGuid();
        var checkoutRequest = new CheckoutRequest
        {
            Id = id,
            RequestContextId = checkoutRequestContextId,
            RequestingUser = requestingUser,
            RespondingUser = respondingCheckout,
            RequestedDateTime = DateTime.UtcNow
        };

        var cacheKey = $"{_checkoutRequestCacheKey}{checkoutRequestContextId}";
        if (!await _checkOutRequestsCache.TrySetAsync(cacheKey, checkoutRequest))
        {
            return null;
        }

        return checkoutRequest;
    }

    ///<inheritdoc />
    public async Task<CheckoutRequest?> TryRemoveCheckoutRequestAsync(Guid checkoutRequestContextId)
    {
        var cacheKey = $"{_checkoutRequestCacheKey}{checkoutRequestContextId}";
        var request = await _checkOutRequestsCache.GetAsync(cacheKey);
        if (request == null)
        {
            return null;
        }

        await _checkOutRequestsCache.RemoveAsync(cacheKey);
        return request;
    }

    ///<inheritdoc />
    public async Task<bool> HasCheckoutBeenRequestedAsync(int id)
    {
        var allRequests = await _checkOutRequestsCache.GetAllValuesAsync(_checkoutRequestCacheKey).ToListAsync();
        return allRequests.Any(t => t?.Id == id);
    }

    #endregion Request Checkouts

    #region Gets

    ///<inheritdoc />
    public async Task<IReadOnlyList<PUSpecSheetCheckout>> GetAllPUSpecSheetCheckoutsAsync()
    {
        var puSpecSheetCheckouts = new List<PUSpecSheetCheckout>();

        var allKeys = await _checkedOutPUSpecSheetsCache.GetAllKeysAsync(_checkoutCacheKey).ToListAsync();
        foreach (var key in allKeys)
        {
            var user = await _checkedOutPUSpecSheetsCache.GetAsync(key);
            if (user != null)
            {
                var keyWithoutPrefix = key.Replace(_checkoutCacheKey, string.Empty);
                var ids = keyWithoutPrefix.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (ids.Length != 1)
                {
                    continue;
                }

                if (!int.TryParse(ids[0], out var puSpecSheetId))
                {
                    continue;
                }

                var checkout = new PUSpecSheetCheckout
                {
                    Id = puSpecSheetId,
                    User = user
                };
                puSpecSheetCheckouts.Add(checkout);
            }
        }

        return puSpecSheetCheckouts;
    }

    ///<inheritdoc />
    public async Task<CheckoutRequest?> GetCheckoutRequestByIdAsync(Guid requestContextId)
    {
        var cacheKey = $"{_checkoutRequestCacheKey}{requestContextId}";
        return await _checkOutRequestsCache.GetAsync(cacheKey);
    }

    ///<inheritdoc />
    public async Task<IReadOnlyList<CheckoutRequest>> GetAllCheckoutRequestsForUserAsync(string connectionId)
    {
        var allRequests = await _checkOutRequestsCache.GetAllValuesAsync(_checkoutRequestCacheKey).ToListAsync();
        return [.. allRequests.Where(t => t?.RequestingUser.ConnectionId == connectionId || t?.RespondingUser.ConnectionId == connectionId)];
    }

    private async Task<ConnectedUser?> GetCheckedOutUserByIdAsync(int id)
    {
        var cacheKey = $"{_checkoutCacheKey}:{id}";
        return await _checkedOutPUSpecSheetsCache.GetAsync(cacheKey);
    }

    #endregion Gets
}