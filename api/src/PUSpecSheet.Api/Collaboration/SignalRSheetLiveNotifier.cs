using Microsoft.AspNetCore.SignalR;
using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Api.Collaboration;

/// <summary>Reaches the browsers that have a sheet open through <see cref="SheetHub"/>.</summary>
public sealed class SignalRSheetLiveNotifier(IHubContext<SheetHub, ISheetHubClient> hub) : ISheetLiveNotifier
{
    public Task PresenceChangedAsync(int sheetId, IReadOnlyList<SheetPresenceUserDto> users, CancellationToken cancellationToken)
    {
        return hub.Clients.Group(SheetGroups.Sheet(sheetId)).PresenceChanged(users);
    }

    public Task CheckoutsChangedAsync(int sheetId, IReadOnlyList<RowCheckoutDto> checkouts, CancellationToken cancellationToken)
    {
        return hub.Clients.Group(SheetGroups.Sheet(sheetId)).CheckoutsChanged(checkouts);
    }

    public Task SheetChangedAsync(int sheetId, string? exceptConnectionId, CancellationToken cancellationToken)
    {
        var group = SheetGroups.Sheet(sheetId);
        var clients = string.IsNullOrEmpty(exceptConnectionId)
            ? hub.Clients.Group(group)
            : hub.Clients.GroupExcept(group, exceptConnectionId);
        return clients.SheetChanged();
    }

    public Task TakeoverChangedAsync(RowTakeoverDto takeover, CancellationToken cancellationToken)
    {
        return hub.Clients
            .Groups(
                SheetGroups.User(takeover.SheetId, takeover.RequesterUserId),
                SheetGroups.User(takeover.SheetId, takeover.HolderUserId))
            .TakeoverChanged(takeover);
    }
}
