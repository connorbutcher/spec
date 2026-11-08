using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Templates;

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

    [Fact]
    public async Task RowsChangingHands_AreNamed_AndAVersionMeansEverything()
    {
        await announcer.AnnounceAsync(Sheet(latestVersion: 3), null, CancellationToken.None);
        await announcer.AnnounceAsync(Sheet(latestVersion: 3, lockedRow: 10), null, CancellationToken.None);
        await announcer.AnnounceAsync(Sheet(latestVersion: 4), null, CancellationToken.None);

        Assert.Equal(["*", "10", "*"], notifier.Notices);
    }

    [Fact]
    public async Task ARowAReadTookNoteOf_IsStillNamedInTheNextAnnouncement()
    {
        await announcer.AnnounceAsync(Sheet(latestVersion: 3, lockedRow: 10), null, CancellationToken.None);

        // A read dropped row 10's draft without telling anyone; then someone takes row 11.
        announcer.Observe(Sheet(latestVersion: 3));
        await announcer.AnnounceAsync(Sheet(latestVersion: 3, lockedRow: 11), null, CancellationToken.None);

        Assert.Equal(["*", "10,11"], notifier.Notices);
    }

    private static SheetDto Sheet(int latestVersion, bool isLive = true, int? lockedRow = null)
    {
        if (lockedRow is { } rowId)
        {
            var row = new SheetRowDto(rowId, Guid.Empty, 1, 1, new SheetLockDto(2, "Engineer Two", IsMine: false), false, CanRemove: true, [], null);
            var section = new SheetSectionDto(1, Guid.Empty, 1, "Limits", SectionRole.Header, 1, 1, 1, 1, null, false, false, [row], [], [], [], null);
            var table = new SheetTableDto(1, Guid.Empty, 1, "Limits", 1, TemplateOrientation.Vertical, 0, null, 1, null, false, [section], [], [], []);
            return new SheetDto(SheetId, Guid.Empty, 1, 1, isLive, null, null, latestVersion, 0, [], [table], []);
        }

        return new SheetDto(SheetId, Guid.Empty, 1, 1, isLive, null, null, latestVersion, 0, [], [], []);
    }
}
