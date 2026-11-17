using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Templates;
using PUSpecSheet.DataAccess.Sheets.Collaboration;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Collaboration;

/// <summary>Others are told about a change only when it is one they can see.</summary>
public sealed class SheetChangeAnnouncerTests
{
    private const int SheetId = 1;

    private readonly RecordingSheetLiveNotifier _notifier = new();
    private readonly SheetChangeAnnouncer _announcer;

    public SheetChangeAnnouncerTests()
    {
        _announcer = new SheetChangeAnnouncer(_notifier);
    }

    [Fact]
    public async Task AChangeOthersCanSee_IsAnnouncedOnce()
    {
        await _announcer.AnnounceAsync(Sheet(latestVersion: 3), null, CancellationToken.None);
        await _announcer.AnnounceAsync(Sheet(latestVersion: 3), null, CancellationToken.None);
        await _announcer.AnnounceAsync(Sheet(latestVersion: 4), null, CancellationToken.None);

        Assert.Equal([SheetId, SheetId], _notifier.ChangedSheets);
    }

    [Fact]
    public async Task AReadIsNeverAnnounced_ButIsRemembered()
    {
        await _announcer.AnnounceAsync(Sheet(latestVersion: 3), null, CancellationToken.None);

        // A read can find the sheet different (it drops the reader's no-op drafts) without telling anyone...
        _announcer.Observe(Sheet(latestVersion: 4));
        Assert.Single(_notifier.ChangedSheets);

        // ...so a change back to what was last announced still counts as a change.
        await _announcer.AnnounceAsync(Sheet(latestVersion: 3), null, CancellationToken.None);
        Assert.Equal(2, _notifier.ChangedSheets.Count);
    }

    [Fact]
    public async Task APastView_IsNeverAnnounced()
    {
        await _announcer.AnnounceAsync(Sheet(latestVersion: 3, isLive: false), null, CancellationToken.None);

        Assert.Empty(_notifier.ChangedSheets);
    }

    [Fact]
    public async Task RowsChangingHands_AreNamed_AndAVersionMeansEverything()
    {
        await _announcer.AnnounceAsync(Sheet(latestVersion: 3), null, CancellationToken.None);
        await _announcer.AnnounceAsync(Sheet(latestVersion: 3, lockedRow: 10), null, CancellationToken.None);
        await _announcer.AnnounceAsync(Sheet(latestVersion: 4), null, CancellationToken.None);

        Assert.Equal(["*", "10", "*"], _notifier.Notices);
    }

    [Fact]
    public async Task ARowAReadTookNoteOf_IsStillNamedInTheNextAnnouncement()
    {
        await _announcer.AnnounceAsync(Sheet(latestVersion: 3, lockedRow: 10), null, CancellationToken.None);

        // A read dropped row 10's draft without telling anyone; then someone takes row 11.
        _announcer.Observe(Sheet(latestVersion: 3));
        await _announcer.AnnounceAsync(Sheet(latestVersion: 3, lockedRow: 11), null, CancellationToken.None);

        Assert.Equal(["*", "10,11"], _notifier.Notices);
    }

    private static SheetDto Sheet(int latestVersion, bool isLive = true, int? lockedRow = null)
    {
        if (lockedRow is { } rowId)
        {
            var row = new SheetRowDto(rowId, Guid.Empty, 1, 1, new SheetLockDto(2, "Engineer Two", isMine: false), false, canRemove: true, [], null);
            var section = new SheetSectionDto(1, Guid.Empty, 1, "Limits", SectionRole.Header, 1, 1, 1, 1, null, false, false, [row], [], [], null);
            var table = new SheetTableDto(1, Guid.Empty, 1, "Limits", 1, TemplateOrientation.Vertical, 0, null, 1, null, false, [section], [], [], []);
            return new SheetDto(SheetId, Guid.Empty, 1, 1, isLive, null, null, latestVersion, 0, [], [table], []);
        }

        return new SheetDto(SheetId, Guid.Empty, 1, 1, isLive, null, null, latestVersion, 0, [], [], []);
    }
}
