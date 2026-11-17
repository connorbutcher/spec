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

    private static Sheet Sheet(int latestVersion, bool isLive = true, int? lockedRow = null)
    {
        if (lockedRow is { } rowId)
        {
            var row = new SheetRow
            {
                Id = rowId,
                PublicId = Guid.Empty,
                TemplateRowId = 1,
                DisplayOrder = 1,
                Lock = new SheetLock { UserId = 2, UserName = "Engineer Two", IsMine = false },
                IsPending = false,
                CanRemove = true,
                Cells = [],
            };
            var section = new SheetSection
            {
                Id = 1,
                PublicId = Guid.Empty,
                TemplateSectionId = 1,
                Name = "Limits",
                Role = SectionRole.Header,
                MinInstances = 1,
                MaxInstances = 1,
                InitialInstances = 1,
                DisplayOrder = 1,
                IsPending = false,
                CanRemove = false,
                Rows = [row],
                Sections = [],
                AddableSections = [],
            };
            var table = new SheetTable
            {
                Id = 1,
                PublicId = Guid.Empty,
                TableTemplateId = 1,
                TemplateName = "Limits",
                TemplateVersionNumber = 1,
                Orientation = TemplateOrientation.Vertical,
                StickyColumnCount = 0,
                DisplayOrder = 1,
                IsPending = false,
                Sections = [section],
                AddableSections = [],
                ColumnBlocks = [],
                AddableColumnBlocks = [],
            };
            return new Sheet
            {
                Id = SheetId,
                PublicId = Guid.Empty,
                PhaseId = 1,
                SheetTypeId = 1,
                IsLive = isLive,
                LatestVersionNumber = latestVersion,
                MyDraftCount = 0,
                Versions = [],
                Tables = [table],
                AvailableTemplates = [],
            };
        }

        return new Sheet
        {
            Id = SheetId,
            PublicId = Guid.Empty,
            PhaseId = 1,
            SheetTypeId = 1,
            IsLive = isLive,
            LatestVersionNumber = latestVersion,
            MyDraftCount = 0,
            Versions = [],
            Tables = [],
            AvailableTemplates = [],
        };
    }
}
