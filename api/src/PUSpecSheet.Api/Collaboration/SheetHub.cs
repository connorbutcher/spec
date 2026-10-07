using Microsoft.AspNetCore.SignalR;
using PUSpecSheet.Api.Users;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Api.Collaboration;

/// <summary>
/// The live connection a browser keeps while it has a sheet open. It only says which sheet a connection
/// is looking at; everything that changes a sheet or a checkout goes through the HTTP API, which then
/// tells the connections here. Who a connection is comes from the signed-in user it was opened by.
/// </summary>
public sealed class SheetHub(ISheetPresenceService presence) : Hub<ISheetHubClient>
{
    /// <summary>Starts watching a sheet, in place of any sheet this connection was watching.</summary>
    public async Task<SheetLiveStateDto> JoinSheet(int sheetId)
    {
        await LeaveSheet();

        var userId = Context.User?.FindUserId() ?? throw new HubException("You are not signed in.");
        SheetLiveStateDto state;
        try
        {
            state = await presence.JoinAsync(Context.ConnectionId, sheetId, userId, Context.ConnectionAborted);
        }
        catch (NotFoundException exception)
        {
            throw new HubException(exception.Message);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, SheetGroups.Sheet(sheetId), Context.ConnectionAborted);
        await Groups.AddToGroupAsync(Context.ConnectionId, SheetGroups.User(sheetId, userId), Context.ConnectionAborted);
        return state;
    }

    /// <summary>Stops watching the sheet this connection has open, if it has one.</summary>
    public async Task LeaveSheet()
    {
        var left = await presence.LeaveAsync(Context.ConnectionId, Context.ConnectionAborted);
        if (left is null)
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SheetGroups.Sheet(left.SheetId), Context.ConnectionAborted);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SheetGroups.User(left.SheetId, left.UserId), Context.ConnectionAborted);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // The connection has gone, and its groups with it; the others still need to hear that it left.
        await presence.LeaveAsync(Context.ConnectionId, CancellationToken.None);
        await base.OnDisconnectedAsync(exception);
    }
}
