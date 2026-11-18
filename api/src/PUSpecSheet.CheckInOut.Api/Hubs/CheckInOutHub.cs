using Microsoft.AspNetCore.SignalR;
using PUSpecSheet.CheckInOut.Api.Models;
using PUSpecSheet.CheckInOut.Api.Services.Presence;
using PUSpecSheet.CheckInOut.Api.Services.Users;

namespace PUSpecSheet.CheckInOut.Api.Hubs;

/// <summary>
/// Who is viewing which sheet, and notices of changes to what is checked out on it. Rows and tables are checked out in
/// the API's database (each draft belongs to the user who made it), so a checkout outlives a connection; this hub only
/// tells the people viewing a sheet when to read it again.
/// </summary>
public class CheckInOutHub : Hub<ICheckInOutHub>
{
    private const string User = "access_token";

    private readonly ILogger<CheckInOutHub> _logger;
    private readonly IConnectedUserService _connectedUserService;
    private readonly ISheetPresenceService _sheetPresenceService;

    public CheckInOutHub(IConnectedUserService connectedUserService,
        ISheetPresenceService sheetPresenceService,
        ILogger<CheckInOutHub> logger)
    {
        ArgumentNullException.ThrowIfNull(connectedUserService);
        ArgumentNullException.ThrowIfNull(sheetPresenceService);
        ArgumentNullException.ThrowIfNull(logger);

        _connectedUserService = connectedUserService;
        _sheetPresenceService = sheetPresenceService;
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
        await base.OnConnectedAsync();
    }

    ///<inheritdoc />
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Disconnected");

        var sheetIds = await _sheetPresenceService.LeaveAllAsync(Context.ConnectionId);
        await _connectedUserService.RemoveConnectedUserAsync(Context.ConnectionId);

        foreach (var sheetId in sheetIds)
        {
            await Clients.Group(SheetGroup(sheetId)).SheetPresence(await _sheetPresenceService.GetPresenceAsync(sheetId));
        }

        await base.OnDisconnectedAsync(exception);
    }

    #endregion Overrideables

    #region Presence

    /// <summary>
    /// Records that the caller has the sheet open, and tells everyone viewing it.
    /// </summary>
    /// <param name="sheetId">The id of the sheet.</param>
    public async Task JoinSheetAsync(int sheetId)
    {
        var user = await GetCallerAsync(sheetId);

        await Groups.AddToGroupAsync(Context.ConnectionId, SheetGroup(sheetId));
        await _sheetPresenceService.JoinAsync(sheetId, user);

        await Clients.Group(SheetGroup(sheetId)).SheetPresence(await _sheetPresenceService.GetPresenceAsync(sheetId));
    }

    /// <summary>
    /// Records that the caller has closed the sheet, and tells the others viewing it.
    /// </summary>
    /// <param name="sheetId">The id of the sheet.</param>
    public async Task LeaveSheetAsync(int sheetId)
    {
        if (sheetId <= 0)
        {
            throw new ArgumentException("Cannot leave a sheet with an invalid id", nameof(sheetId));
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SheetGroup(sheetId));
        await _sheetPresenceService.LeaveAsync(sheetId, Context.ConnectionId);

        await Clients.Group(SheetGroup(sheetId)).SheetPresence(await _sheetPresenceService.GetPresenceAsync(sheetId));
    }

    #endregion Presence

    #region Changes

    /// <summary>
    /// Tells the others viewing the sheet that the caller changed what is checked out on it.
    /// </summary>
    /// <param name="sheetId">The id of the sheet.</param>
    /// <param name="change">What the caller did, in a few words to show.</param>
    public async Task SheetChangedAsync(int sheetId, string change)
    {
        var user = await GetCallerAsync(sheetId);

        _logger.LogInformation("Sheet {SheetId} changed by {ConnectionId}: {Change}", sheetId, user.ConnectionId, change);
        await Clients.OthersInGroup(SheetGroup(sheetId)).SheetChanged(new SheetChangedNotice
        {
            SheetId = sheetId,
            User = user,
            Change = string.IsNullOrWhiteSpace(change) ? "changed the sheet" : change
        });
    }

    #endregion Changes

    private async Task<ConnectedUser> GetCallerAsync(int sheetId)
    {
        if (sheetId <= 0)
        {
            _logger.LogError("Invalid sheet id {Id}", sheetId);
            throw new ArgumentException("Cannot use a sheet with an invalid id", nameof(sheetId));
        }

        var user = await _connectedUserService.GetConnectedUserAsync(Context.ConnectionId);
        if (user == null)
        {
            _logger.LogError("No connected user for {ConnectionId}", Context.ConnectionId);
            throw new InvalidOperationException("Cannot use a sheet without a connected user");
        }

        return user;
    }

    private static string SheetGroup(int sheetId)
    {
        return $"sheet:{sheetId}";
    }
}
