using Microsoft.AspNetCore.SignalR;
using PUSpecSheet.CheckInOut.Api.Models;
using PUSpecSheet.CheckInOut.Api.Services.SpecSheets;
using PUSpecSheet.CheckInOut.Api.Services.Users;

namespace PUSpecSheet.CheckInOut.Api.Hubs;

public class CheckInOutHub : Hub<ICheckInOutHub>
{
    private const string User = "access_token";

    private readonly ILogger<CheckInOutHub> _logger;
    private readonly IConnectedUserService _connectedUserService;
    private readonly IPUSpecSheetCheckInOutService _puSpecSheetCheckInOutService;

    public CheckInOutHub(IConnectedUserService connectedUserService,
        IPUSpecSheetCheckInOutService puSpecSheetCheckInOutService,
        ILogger<CheckInOutHub> logger)
    {
        ArgumentNullException.ThrowIfNull(connectedUserService);
        ArgumentNullException.ThrowIfNull(puSpecSheetCheckInOutService);
        ArgumentNullException.ThrowIfNull(logger);

        _connectedUserService = connectedUserService;
        _puSpecSheetCheckInOutService = puSpecSheetCheckInOutService;
        _logger = logger;
    }

    #region Overrideables

    ///<inheritdoc />
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("OnConnectedAsync method called.");

        var httpContext = Context.GetHttpContext();
        if (httpContext == null)
        {
            _logger.LogError("No http context found for request.");
            return;
        }

        if (!httpContext.Request.Query.TryGetValue(User, out var usernameValue) || usernameValue.Count == 0)
        {
            _logger.LogError("No username header found for request.");
            return;
        }

        var usernameAndUid = usernameValue.FirstOrDefault() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(usernameAndUid))
        {
            _logger.LogError("No username found in header.");
            return;
        }

        var split = usernameAndUid.Split(':');
        var username = split[0];
        var uid = "empty";
        if (split.Length > 1)
        {
            uid = split[1];
        }

        await _connectedUserService.TryAddUserAsync(Context.ConnectionId, username, uid);

        var checkoutSummary = await GetCheckoutSummaryAsync();
        await Clients.Client(Context.ConnectionId).CheckInOutSummary(checkoutSummary);
        await base.OnConnectedAsync();
    }

    ///<inheritdoc />
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Disconnected");

        var checkoutRequestsForUser = await _puSpecSheetCheckInOutService.GetAllCheckoutRequestsForUserAsync(Context.ConnectionId);
        foreach (var checkoutRequestForUser in checkoutRequestsForUser)
        {
            // If the user is the requestor, cancel all requests.
            if (checkoutRequestForUser.RequestingUser.ConnectionId == Context.ConnectionId)
            {
                await CancelRequestToCheckoutAsync(checkoutRequestForUser.RequestContextId);
                continue;
            }

            // If the user is the responder, approve the requests.
            if (checkoutRequestForUser.RespondingUser.ConnectionId == Context.ConnectionId)
            {
                await RespondToCheckoutRequestAsync(checkoutRequestForUser.RequestContextId, true);
            }
        }

        await _puSpecSheetCheckInOutService.TryRemoveAllCheckInsForUserAsync(Context.ConnectionId);
        await _connectedUserService.RemoveConnectedUserAsync(Context.ConnectionId);

        var checkoutSummary = await GetCheckoutSummaryAsync();
        await Clients.All.CheckInOutSummary(checkoutSummary);
        await base.OnDisconnectedAsync(exception);
    }

    #endregion Overrideables

    #region Check Out

    /// <summary>
    /// Checkouts a pu spec sheet.
    /// </summary>
    /// <param name="id">The id of the pu spec sheet.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public async Task CheckoutAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogError("Cannot checkout item with invalid id {Id}", id);
            throw new ArgumentException("Cannot checkout an item with an invalid id", nameof(id));
        }

        var user = await _connectedUserService.GetConnectedUserAsync(Context.ConnectionId);
        if (user == null)
        {
            _logger.LogError("Cannot checkout an item without a user");
            throw new ArgumentException("Cannot checkout an item without a user", nameof(user));
        }


        _logger.LogInformation("Checking out item: {Id} for user: {ConnectionId}", id, user.ConnectionId);
        if (!await _puSpecSheetCheckInOutService.TryCheckoutPUSpecSheetAsync(id, user))
        {
            _logger.LogError("PU Spec Sheet: {Id} is already checked out", id);
            throw new InvalidOperationException($"Cannot checkout a pu spec sheet that is already checked out. Id: {id}");
        }

        var checkoutSummary = await GetCheckoutSummaryAsync();
        await Clients.All.CheckInOutSummary(checkoutSummary);
    }

    #endregion Check Out

    #region Check In

    /// <summary>
    /// Check in a pu spec sheet.
    /// </summary>
    /// <param name="id">The id of the pu spec sheet.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public async Task CheckInAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogError("Cannot checkout item with invalid id {Id}", id);
            throw new ArgumentException("Cannot checkout an item with an invalid id", nameof(id));
        }

        var user = await _connectedUserService.GetConnectedUserAsync(Context.ConnectionId);
        if (user == null)
        {
            _logger.LogError("Cannot checkout an item without a user");
            throw new ArgumentException("Cannot checkout an item without a user", nameof(user));
        }


        _logger.LogInformation("Checking in template: {Id} for user: {ConnectionId}", id, user.ConnectionId);
        if (!await _puSpecSheetCheckInOutService.TryCheckInPUSpecSheetAsync(id, user.ConnectionId))
        {
            _logger.LogError("PU Spec Sheet: {Id} is not checked out", id);
            throw new InvalidOperationException($"Cannot check in a PU Spec Sheet that is not checked out. Id: {id}");
        }

        var checkoutSummary = await GetCheckoutSummaryAsync();
        await Clients.All.CheckInOutSummary(checkoutSummary);
    }

    #endregion Check In

    #region Request Checkouts

    /// <summary>
    /// Request a checkout for a template.
    /// </summary>        
    /// <param name="id">The template id.</param>
    /// <returns>The checkout request.</returns>
    public async Task<CheckoutRequest> RequestCheckoutAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogError("Cannot checkout template with invalid id {Id}", id);
            throw new ArgumentException("Cannot checkout an item with an invalid id", nameof(id));
        }

        var user = await _connectedUserService.GetConnectedUserAsync(Context.ConnectionId);
        if (user == null)
        {
            _logger.LogError("Cannot checkout an item without a user");
            throw new ArgumentException("Cannot checkout an item without a user", nameof(user));
        }

        if (await _puSpecSheetCheckInOutService.HasCheckoutBeenRequestedAsync(id))
        {
            _logger.LogError("Cannot request checkout of item that has already been requested");
            throw new Exception("Cannot request checkout of item that has already been requested");
        }

        var checkoutRequest = await _puSpecSheetCheckInOutService.RequestCheckoutAsync(id, user);
        if (checkoutRequest == null)
        {
            _logger.LogError("Cannot request checkout of item that is not checked out");
            throw new Exception("Cannot request checkout of item that is not checked out");
        }

        // Inform the responding user that the checkout of an item has been requested.
        await Clients.Client(checkoutRequest.RespondingUser.ConnectionId).CheckoutRequested(checkoutRequest);
        return checkoutRequest;
    }

    /// <summary>
    /// Override a request to checkout an item.
    /// </summary>
    /// <param name="checkoutRequestContextId">The request context id.</param>
    public async Task OverrideCheckoutRequestAsync(Guid checkoutRequestContextId)
    {
        if (checkoutRequestContextId == Guid.Empty)
        {
            _logger.LogError("Cannot override a request without a context id");
            throw new ArgumentException("Cannot override a request without a context id", nameof(checkoutRequestContextId));
        }

        var requestingUser = await _connectedUserService.GetConnectedUserAsync(Context.ConnectionId);
        if (requestingUser == null)
        {
            _logger.LogError("Cannot override a request without a user");
            throw new ArgumentException("Cannot override a request without a user", nameof(requestingUser));
        }

        var request = await _puSpecSheetCheckInOutService.TryRemoveCheckoutRequestAsync(checkoutRequestContextId);
        if (request == null)
        {
            _logger.LogError("Could not override request to checkout item: {id}", request?.Id);
            throw new InvalidOperationException($"Could not override request to checkout item: {request?.Id}");
        }

        var hasCheckedIn = await _puSpecSheetCheckInOutService.TryCheckInPUSpecSheetAsync(request.Id, request.RespondingUser.ConnectionId);
        if (!hasCheckedIn)
        {
            _logger.LogError("Cannot check in an item that is not checked out {id}", request.Id);
            throw new InvalidOperationException($"Cannot check in an item that is not checked out. {request.Id}");
        }

        await Clients.Client(request.RespondingUser.ConnectionId).OverrideCheckoutRequested(request);

        if (!await _puSpecSheetCheckInOutService.TryCheckoutPUSpecSheetAsync(request.Id, request.RequestingUser))
        {
            _logger.LogError("Item: {id} is already checked out", request.Id);
            throw new InvalidOperationException($"Cannot checkout an item that is already checked out. {request.Id}");
        }

        var checkoutSummary = await GetCheckoutSummaryAsync();
        await Clients.All.CheckInOutSummary(checkoutSummary);
    }

    /// <summary>
    /// Cancel a request to checkout an item.
    /// </summary>
    /// <param name="checkoutRequestId">The checkout request context id.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="checkoutRequestId"/> is <see cref="Guid.Empty"/>. Can also be thrown if there is no user context.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the request cannot be cancelled. Likely due to the request not existing.</exception>
    public async Task CancelRequestToCheckoutAsync(Guid checkoutRequestId)
    {
        if (checkoutRequestId == Guid.Empty)
        {
            _logger.LogError("Cannot cancel a request without a context id");
            throw new ArgumentException("Cannot cancel a request without a context id", nameof(checkoutRequestId));
        }

        var requestingUser = await _connectedUserService.GetConnectedUserAsync(Context.ConnectionId);
        if (requestingUser == null)
        {
            _logger.LogError("Cannot cancel a request without a user");
            throw new ArgumentException("Cannot cancel a request without a user", nameof(requestingUser));
        }

        var request = await _puSpecSheetCheckInOutService.TryRemoveCheckoutRequestAsync(checkoutRequestId);
        if (request == null)
        {
            _logger.LogError("Could not cancel request to checkout item: {id}", request?.Id);
            throw new InvalidOperationException($"Could not cancel request to checkout item: {request?.Id}");
        }

        await Clients.Client(request.RespondingUser.ConnectionId).CancelCheckoutRequest(checkoutRequestId);
    }

    /// <summary>
    /// Respond to a request to checkout an item.
    /// </summary>
    /// <param name="checkoutRequestId">The checkout request context id.</param>
    /// <param name="accept">Whether the user has accepted or rejected the request.</param>
    public async Task RespondToCheckoutRequestAsync(Guid checkoutRequestId, bool accept)
    {
        if (checkoutRequestId == Guid.Empty)
        {
            _logger.LogError("Cannot respond to a table request without a context id");
            throw new ArgumentException("Cannot respond to a table request without a context id", nameof(checkoutRequestId));
        }

        var respondingUser = await _connectedUserService.GetConnectedUserAsync(Context.ConnectionId);
        if (respondingUser == null)
        {
            _logger.LogError("Cannot respond to a table request without a user");
            throw new ArgumentException("Cannot respond to a table request without a user", nameof(respondingUser));
        }

        // Get the request
        var request = await _puSpecSheetCheckInOutService.GetCheckoutRequestByIdAsync(checkoutRequestId);
        if (request == null)
        {
            _logger.LogError("Could not find request to respond to: {checkoutRequestContextId}", checkoutRequestId);
            throw new InvalidOperationException($"Could not find request to respond to: {checkoutRequestId}");
        }

        if (accept)
        {
            // Check in the table for the responding user.
            var hasCheckedIn = await _puSpecSheetCheckInOutService.TryCheckInPUSpecSheetAsync(request.Id, request.RespondingUser.ConnectionId);
            if (!hasCheckedIn)
            {
                _logger.LogError("Item {id} is not checked in", request.Id);
                throw new InvalidOperationException($"Cannot checkin a item that is not checked in. {request.Id}");
            }

            // Check out the table for the requesting user.
            if (!await _puSpecSheetCheckInOutService.TryCheckoutPUSpecSheetAsync(request.Id, request.RequestingUser))
            {
                _logger.LogError("Item: {id} is already checked out", request?.Id);
                throw new InvalidOperationException($"Cannot checkout a item that is already checked out. {request?.Id}");
            }

            _logger.LogInformation("Accepting request to checkout item: {id}", request.Id);
            await Clients.Client(request.RequestingUser.ConnectionId).CheckoutRequestAccepted(request);
        }
        else
        {
            _logger.LogInformation("Rejecting request to checkout item: {id}", request.Id);
            await Clients.Client(request.RequestingUser.ConnectionId).CheckoutRequestRejected(request);
        }

        var removed = await _puSpecSheetCheckInOutService.TryRemoveCheckoutRequestAsync(checkoutRequestId);
        if (removed == null)
        {
            _logger.LogError("Could not override request to checkout item: {id}", request?.Id);
            throw new InvalidOperationException($"Could not override request to checkout item: {request?.Id}");
        }


        var checkoutSummary = await GetCheckoutSummaryAsync();
        await Clients.All.CheckInOutSummary(checkoutSummary);
    }

    #endregion Request Checkouts

    //#region Saved

    ///// <summary>
    ///// Notify users that a pu card has been saved.
    ///// </summary>
    ///// <param name="id">The id of the pu card.</param>
    //public async Task PUCardSavedAsync(int id)
    //{
    //    if (id <= 0)
    //    {
    //        _logger.LogError("Cannot inform users of a save without an id");
    //        throw new ArgumentException("Cannot inform users of a save without an id", nameof(id));
    //    }

    //    var requestingUser = await _connectedUserService.GetConnectedUserAsync(Context.ConnectionId);
    //    if (requestingUser == null)
    //    {
    //        _logger.LogError("Cannot inform users of a save without a user");
    //        throw new ArgumentException("Cannot inform users of a save without a user", nameof(requestingUser));
    //    }

    //    // Get all the checkouts for the user and id.
    //    var puCardCheckout = await _puCardCheckInOutService.GetPUCardUserCheckoutForUserAsync(Context.ConnectionId, id);
    //    if (puCardCheckout == null)
    //    {
    //        _logger.LogError("Could not find pu card checkout for user: {connectionId} and id: {id}", Context.ConnectionId, id);
    //        throw new InvalidOperationException($"Could not find pu card checkout for user: {Context.ConnectionId} and id: {id}");
    //    }

    //    var tables = puCardCheckout.PUCardTables
    //        .Select(t => t.TableId)
    //        .ToList();
    //    // Check in all the tables for the user.
    //    foreach (var table in tables)
    //    {
    //        await CheckInTableAsync(id, table);
    //    }

    //    // Inform all the users with that pu card open that the pu card has been saved, except the user who saved it.
    //    await Clients.GroupExcept($"{Enum.GetName(puSpecSheetTypeId.PUCard)}:{id}", Context.ConnectionId).PUCardSaved(id, requestingUser.Username);
    //}

    //#endregion Saved

    //#region Locking

    ///// <summary>
    ///// Locks a pu card.
    ///// </summary>
    ///// <param name="id">The id of the locked pu card.</param>
    //public async Task PUCardLockedAsync(int id)
    //{
    //    if (id <= 0)
    //    {
    //        _logger.LogError("Cannot lock a pu card without an id");
    //        throw new ArgumentException("Cannot lock a pu card without an id", nameof(id));
    //    }

    //    var requestingUser = await _connectedUserService.GetConnectedUserAsync(Context.ConnectionId);
    //    if (requestingUser == null)
    //    {
    //        _logger.LogError("Cannot lock a pu card without a user");
    //        throw new ArgumentException("Cannot lock a pu card without a user", nameof(requestingUser));
    //    }

    //    _logger.LogInformation("Locking pu card: {jobNum}", id);

    //    var checkoutForUser = await _puCardCheckInOutService.GetPUCardUserCheckoutForUserAsync(requestingUser.ConnectionId, id);
    //    if (checkoutForUser == null)
    //    {
    //        _logger.LogError("PU card: {jobNum} is not checked in", id);
    //        throw new InvalidOperationException($"Cannot lock a pu card that is not checked out. Id: {id}");
    //    }

    //    foreach (var table in checkoutForUser.PUCardTables)
    //    {
    //        await CheckInTableAsync(id, table.TableId);
    //    }

    //    await Clients.GroupExcept($"{Enum.GetName(puSpecSheetTypeId.PUCard)}:{id}", Context.ConnectionId).PUCardLocked(id, requestingUser.Username);
    //}

    //#endregion Locking

    #region Transfer of Ownership

    /// <summary>
    /// Transfer the ownership of one connection ids checkouts to anothers.
    /// </summary>
    /// <param name="oldConnectionId">The old connection id.</param>
    /// <param name="oldConnectionCheckoutSummaryForUser">The old users checkouts.</param>
    public async Task TransferOwnershipAsync(string oldConnectionId, CheckoutSummary oldConnectionCheckoutSummaryForUser)
    {
        if (string.IsNullOrWhiteSpace(oldConnectionId))
        {
            _logger.LogError("Cannot transfer ownership without a connection id");
            return;
        }

        await _connectedUserService.RemoveConnectedUserAsync(oldConnectionId);

        if (oldConnectionCheckoutSummaryForUser == null)
        {
            _logger.LogWarning("No checkouts to transfer");
            return;
        }

        var user = await _connectedUserService.GetConnectedUserAsync(Context.ConnectionId);
        if (user == null)
        {
            _logger.LogError("Cannot transfer checkouts without a user");
            throw new ArgumentException("Cannot transfer checkouts without a user", nameof(user));
        }

        if (oldConnectionCheckoutSummaryForUser.PUSpecSheetCheckouts?.Count > 0)
        {
            foreach (var specSheetCheckout in oldConnectionCheckoutSummaryForUser.PUSpecSheetCheckouts)
            {
                await _puSpecSheetCheckInOutService.TryCheckInPUSpecSheetAsync(specSheetCheckout.Id, oldConnectionId);
                await _puSpecSheetCheckInOutService.TryCheckoutPUSpecSheetAsync(specSheetCheckout.Id, user);
            }
        }

        var checkoutSummary = await GetCheckoutSummaryAsync();
        await Clients.All.CheckInOutSummary(checkoutSummary);
    }

    #endregion Transfer of Ownership

    #region Gets

    public async Task<CheckoutSummary> GetCheckoutSummaryAsync()
    {
        var puSpecSheetCheckouts = await GetAllPUSpecSheetCheckoutsAsync();
        return new CheckoutSummary(puSpecSheetCheckouts);
    }

    /// <summary>
    /// Get all the checkouts.
    /// </summary>
    /// <returns>The checkouts.</returns>
    public Task<IReadOnlyList<PUSpecSheetCheckout>> GetAllPUSpecSheetCheckoutsAsync()
    {
        return _puSpecSheetCheckInOutService.GetAllPUSpecSheetCheckoutsAsync();
    }

    #endregion Gets
}
