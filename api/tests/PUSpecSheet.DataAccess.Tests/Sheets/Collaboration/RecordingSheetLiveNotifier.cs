using PUSpecSheet.Abstractions.Sheets.Collaboration;
using PUSpecSheet.DataAccess.Sheets.Collaboration;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Collaboration;

/// <summary>Remembers what would have been sent to the browsers.</summary>
internal sealed class RecordingSheetLiveNotifier : ISheetLiveNotifier
{
    /// <summary>The status of each takeover message sent, in order.</summary>
    public List<RowTakeoverStatus> Takeovers { get; } = [];

    /// <summary>The sheets everyone was told to read again.</summary>
    public List<int> ChangedSheets { get; } = [];

    /// <summary>Each list of live checkouts sent, in order, as "row:user" pairs.</summary>
    public List<string> Checkouts { get; } = [];

    public Task PresenceChangedAsync(int sheetId, IReadOnlyList<SheetPresenceUser> users, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>The rows each announcement named, as a comma-separated list, or "*" for the whole sheet.</summary>
    public List<string> Notices { get; } = [];

    public Task SheetChangedAsync(int sheetId, SheetChangedNotice notice, string? exceptConnectionId, CancellationToken cancellationToken)
    {
        ChangedSheets.Add(sheetId);
        Notices.Add(notice.RowIds is { } rows ? string.Join(',', rows) : "*");
        return Task.CompletedTask;
    }

    public Task CheckoutsChangedAsync(int sheetId, IReadOnlyList<RowCheckout> checkouts, CancellationToken cancellationToken)
    {
        Checkouts.Add(string.Join(',', checkouts.Select(checkout => $"{checkout.RowId}:{checkout.UserId}")));
        return Task.CompletedTask;
    }

    public Task TakeoverChangedAsync(RowTakeover takeover, CancellationToken cancellationToken)
    {
        Takeovers.Add(takeover.Status);
        return Task.CompletedTask;
    }
}
