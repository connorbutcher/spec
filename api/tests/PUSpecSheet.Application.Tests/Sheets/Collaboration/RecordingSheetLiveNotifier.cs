using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

/// <summary>Remembers what would have been sent to the browsers.</summary>
internal sealed class RecordingSheetLiveNotifier : ISheetLiveNotifier
{
    /// <summary>The status of each takeover message sent, in order.</summary>
    public List<RowTakeoverStatus> Takeovers { get; } = [];

    /// <summary>The sheets everyone was told to read again.</summary>
    public List<int> ChangedSheets { get; } = [];

    public Task PresenceChangedAsync(int sheetId, IReadOnlyList<SheetPresenceUserDto> users, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SheetChangedAsync(int sheetId, string? exceptConnectionId, CancellationToken cancellationToken)
    {
        ChangedSheets.Add(sheetId);
        return Task.CompletedTask;
    }

    public Task TakeoverChangedAsync(RowTakeoverDto takeover, CancellationToken cancellationToken)
    {
        Takeovers.Add(takeover.Status);
        return Task.CompletedTask;
    }
}
