using PUSpecSheet.Abstractions.Sheets.Collaboration;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration;

public interface ISheetLiveNotifier
{
    Task PresenceChangedAsync(int sheetId, IReadOnlyList<SheetPresenceUser> users, CancellationToken cancellationToken);

    Task SheetChangedAsync(int sheetId, SheetChangedNotice notice, string? exceptConnectionId, CancellationToken cancellationToken);

    Task CheckoutsChangedAsync(int sheetId, IReadOnlyList<RowCheckout> checkouts, CancellationToken cancellationToken);

    Task TakeoverChangedAsync(RowTakeover takeover, CancellationToken cancellationToken);
}
