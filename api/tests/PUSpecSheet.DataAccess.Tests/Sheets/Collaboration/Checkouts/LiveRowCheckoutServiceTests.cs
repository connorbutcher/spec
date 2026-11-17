using PUSpecSheet.Abstractions.Sheets.Collaboration;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Collaboration.Checkouts;

/// <summary>
/// Clicking into a row and leaving it again: Sam (user 1) and Alex (user 2) each have sheet 1 open in one
/// tab, and rows 10 and 11 are on it.
/// </summary>
public sealed class LiveRowCheckoutServiceTests
{
    private const int Sheet = 1;
    private const int Sam = 1;
    private const int Alex = 2;

    private readonly FakeRowCheckouts _drafts = new();
    private readonly RecordingSheetLiveNotifier _notifier = new();
    private readonly SheetPresenceTracker _presence = new();
    private readonly LiveRowCheckoutTracker _tracker = new();
    private readonly RowTakeoverStore _takeovers = new();
    private readonly FakeRemovalInProgressGuard _removals = new();
    private readonly LiveRowCheckoutService _service;

    public LiveRowCheckoutServiceTests()
    {
        var settler = new RowTakeoverSettler(_takeovers, new RowTakeoverCloser(_drafts, _notifier), new SettableClock());
        _service = new LiveRowCheckoutService(_drafts, _presence, _tracker, settler, _removals, _notifier);

        _drafts.AddRow(10);
        _drafts.AddRow(11);
        _drafts.AddRow(20, sheetId: 2);
        _presence.Join("sam", new SheetConnection(Sheet, Sam, "Sam"));
        _presence.Join("alex", new SheetConnection(Sheet, Alex, "Alex"));
    }

    [Fact]
    public async Task ClickingIntoARow_ChecksItOut_AndTellsTheSheet()
    {
        await _service.CheckOutAsync("sam", 10, CancellationToken.None);

        Assert.Equal(Sam, _tracker.HolderOf(10)!.UserId);
        Assert.Equal(["10:1"], _notifier.Checkouts);
    }

    [Fact]
    public async Task ARowSomeoneElseIsIn_IsRefusedByName()
    {
        await _service.CheckOutAsync("sam", 10, CancellationToken.None);

        var refused = await Assert.ThrowsAsync<ConflictException>(() => _service.CheckOutAsync("alex", 10, CancellationToken.None));

        Assert.Equal("This row is being edited by Sam.", refused.Message);
        Assert.Equal(Sam, _tracker.HolderOf(10)!.UserId);
    }

    [Fact]
    public async Task ARowSomeoneElseHasChanged_IsRefused_EvenWhenTheyHaveLeftIt()
    {
        _drafts.CheckOut(10, Sheet, Sam);

        var refused = await Assert.ThrowsAsync<ConflictException>(() => _service.CheckOutAsync("alex", 10, CancellationToken.None));

        Assert.Equal("This row is being edited by User 1.", refused.Message);
        Assert.Null(_tracker.HolderOf(10));
    }

    [Fact]
    public async Task MyOwnChangedRow_CanBeClickedBackInto()
    {
        _drafts.CheckOut(10, Sheet, Sam);

        await _service.CheckOutAsync("sam", 10, CancellationToken.None);

        Assert.Equal(Sam, _tracker.HolderOf(10)!.UserId);
    }

    [Fact]
    public async Task ARowInASectionSomeoneElseIsRemoving_IsRefusedByName()
    {
        _removals.RemoveSectionOf(10, Sam, "Sam");

        var refused = await Assert.ThrowsAsync<ConflictException>(() => _service.CheckOutAsync("alex", 10, CancellationToken.None));

        Assert.Equal("Sam is removing the section this is in, so it can't be changed unless they put it back.", refused.Message);
        Assert.Null(_tracker.HolderOf(10));
        Assert.Empty(_notifier.Checkouts);
    }

    [Fact]
    public async Task MovingToAnotherRow_FreesTheOneBefore()
    {
        await _service.CheckOutAsync("sam", 10, CancellationToken.None);
        await _service.CheckOutAsync("sam", 11, CancellationToken.None);

        Assert.Null(_tracker.HolderOf(10));
        Assert.Equal("11:1", _notifier.Checkouts[^1]);
    }

    [Fact]
    public async Task LeavingARow_FreesIt_AndTellsTheSheet()
    {
        await _service.CheckOutAsync("sam", 10, CancellationToken.None);

        await _service.ReleaseAsync("sam", CancellationToken.None);

        Assert.Null(_tracker.HolderOf(10));
        Assert.Equal(["10:1", string.Empty], _notifier.Checkouts);
    }

    [Fact]
    public async Task LeavingWithNothingHeld_TellsNobody()
    {
        await _service.ReleaseAsync("sam", CancellationToken.None);

        Assert.Empty(_notifier.Checkouts);
    }

    [Fact]
    public async Task ARowThatCannotBeEntered_SaysWhy()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CheckOutAsync("sam", 99, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.CheckOutAsync("sam", 20, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.CheckOutAsync("nobody", 10, CancellationToken.None));
    }

    [Fact]
    public async Task LeavingARowSomeoneAskedFor_ClosesTheirRequestAsReleased()
    {
        // In these tests a draft stands for the checkout the request was made against.
        _drafts.CheckOut(10, Sheet, Sam);
        await _service.CheckOutAsync("sam", 10, CancellationToken.None);
        _takeovers.TryAdd(new RowTakeoverDto(
            Guid.NewGuid(), Sheet, 10, Alex, "Alex", Sam, "Sam", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), RowTakeoverStatus.Pending));

        _drafts.Release(10);
        await _service.ReleaseAsync("sam", CancellationToken.None);

        Assert.Equal([RowTakeoverStatus.Released], _notifier.Takeovers);
        Assert.Null(_takeovers.ForRow(10));
    }
}
