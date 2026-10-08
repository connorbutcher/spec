using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

/// <summary>Others are told about a change only when it is one they can see.</summary>
public sealed class SheetChangeAnnouncerTests
{
    private const int SheetId = 1;

    private readonly RecordingSheetLiveNotifier notifier = new();
    private readonly SheetChangeAnnouncer announcer;

    public SheetChangeAnnouncerTests()
    {
        announcer = new SheetChangeAnnouncer(notifier);
    }

    [Fact]
    public async Task AChangeOthersCanSee_IsAnnouncedOnce()
    {
        await announcer.AnnounceAsync(Sheet(latestVersion: 3), null, CancellationToken.None);
        await announcer.AnnounceAsync(Sheet(latestVersion: 3), null, CancellationToken.None);
        await announcer.AnnounceAsync(Sheet(latestVersion: 4), null, CancellationToken.None);

        Assert.Equal([SheetId, SheetId], notifier.ChangedSheets);
    }

    [Fact]
    public async Task AReadIsNeverAnnounced_ButIsRemembered()
    {
        await announcer.AnnounceAsync(Sheet(latestVersion: 3), null, CancellationToken.None);

        // A read can find the sheet different (it drops the reader's no-op drafts) without telling anyone...
        announcer.Observe(Sheet(latestVersion: 4));
        Assert.Single(notifier.ChangedSheets);

        // ...so a change back to what was last announced still counts as a change.
        await announcer.AnnounceAsync(Sheet(latestVersion: 3), null, CancellationToken.None);
        Assert.Equal(2, notifier.ChangedSheets.Count);
    }

    [Fact]
    public async Task APastView_IsNeverAnnounced()
    {
        await announcer.AnnounceAsync(Sheet(latestVersion: 3, isLive: false), null, CancellationToken.None);

        Assert.Empty(notifier.ChangedSheets);
    }

    private static SheetDto Sheet(int latestVersion, bool isLive = true)
    {
        return new SheetDto(SheetId, Guid.Empty, 1, 1, isLive, null, null, latestVersion, 0, [], [], []);
    }
}
