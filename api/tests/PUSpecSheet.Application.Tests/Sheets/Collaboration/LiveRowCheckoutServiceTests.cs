using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

/// <summary>
/// Clicking into a row and leaving it again: Sam (user 1) and Alex (user 2) each have sheet 1 open in one
/// tab, and rows 10 and 11 are on it.
/// </summary>
public sealed class LiveRowCheckoutServiceTests
{
    private const int Sheet = 1;
    private const int Sam = 1;
    private const int Alex = 2;

    private readonly FakeRowCheckouts drafts = new();
    private readonly RecordingSheetLiveNotifier notifier = new();
    private readonly SheetPresenceTracker presence = new();
    private readonly LiveRowCheckoutTracker tracker = new();
    private readonly RowTakeoverStore takeovers = new();
    private readonly LiveRowCheckoutService service;

    public LiveRowCheckoutServiceTests()
    {
        var settler = new RowTakeoverSettler(takeovers, new RowTakeoverCloser(drafts, notifier), new SettableClock());
        service = new LiveRowCheckoutService(drafts, presence, tracker, settler, notifier);

        drafts.AddRow(10);
        drafts.AddRow(11);
        drafts.AddRow(20, sheetId: 2);
        presence.Join("sam", new SheetConnection(Sheet, Sam, "Sam"));
        presence.Join("alex", new SheetConnection(Sheet, Alex, "Alex"));
    }

    [Fact]
    public async Task ClickingIntoARow_ChecksItOut_AndTellsTheSheet()
    {
        await service.CheckOutAsync("sam", 10, CancellationToken.None);

        Assert.Equal(Sam, tracker.HolderOf(10)!.UserId);
        Assert.Equal(["10:1"], notifier.Checkouts);
    }

    [Fact]
    public async Task ARowSomeoneElseIsIn_IsRefusedByName()
    {
        await service.CheckOutAsync("sam", 10, CancellationToken.None);

        var refused = await Assert.ThrowsAsync<ConflictException>(() => service.CheckOutAsync("alex", 10, CancellationToken.None));

        Assert.Equal("This row is being edited by Sam.", refused.Message);
        Assert.Equal(Sam, tracker.HolderOf(10)!.UserId);
    }

    [Fact]
    public async Task ARowSomeoneElseHasChanged_IsRefused_EvenWhenTheyHaveLeftIt()
    {
        drafts.CheckOut(10, Sheet, Sam);

        var refused = await Assert.ThrowsAsync<ConflictException>(() => service.CheckOutAsync("alex", 10, CancellationToken.None));

        Assert.Equal("This row is being edited by User 1.", refused.Message);
        Assert.Null(tracker.HolderOf(10));
    }

    [Fact]
    public async Task MyOwnChangedRow_CanBeClickedBackInto()
    {
        drafts.CheckOut(10, Sheet, Sam);

        await service.CheckOutAsync("sam", 10, CancellationToken.None);

        Assert.Equal(Sam, tracker.HolderOf(10)!.UserId);
    }

    [Fact]
    public async Task MovingToAnotherRow_FreesTheOneBefore()
    {
        await service.CheckOutAsync("sam", 10, CancellationToken.None);
        await service.CheckOutAsync("sam", 11, CancellationToken.None);

        Assert.Null(tracker.HolderOf(10));
        Assert.Equal("11:1", notifier.Checkouts[^1]);
    }

    [Fact]
    public async Task LeavingARow_FreesIt_AndTellsTheSheet()
    {
        await service.CheckOutAsync("sam", 10, CancellationToken.None);

        await service.ReleaseAsync("sam", CancellationToken.None);

        Assert.Null(tracker.HolderOf(10));
        Assert.Equal(["10:1", string.Empty], notifier.Checkouts);
    }

    [Fact]
    public async Task LeavingWithNothingHeld_TellsNobody()
    {
        await service.ReleaseAsync("sam", CancellationToken.None);

        Assert.Empty(notifier.Checkouts);
    }

    [Fact]
    public async Task ARowThatCannotBeEntered_SaysWhy()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => service.CheckOutAsync("sam", 99, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CheckOutAsync("sam", 20, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CheckOutAsync("nobody", 10, CancellationToken.None));
    }

    [Fact]
    public async Task LeavingARowSomeoneAskedFor_ClosesTheirRequestAsReleased()
    {
        // In these tests a draft stands for the checkout the request was made against.
        drafts.CheckOut(10, Sheet, Sam);
        await service.CheckOutAsync("sam", 10, CancellationToken.None);
        takeovers.TryAdd(new RowTakeoverDto(
            Guid.NewGuid(), Sheet, 10, Alex, "Alex", Sam, "Sam", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), RowTakeoverStatus.Pending));

        drafts.Release(10);
        await service.ReleaseAsync("sam", CancellationToken.None);

        Assert.Equal([RowTakeoverStatus.Released], notifier.Takeovers);
        Assert.Null(takeovers.ForRow(10));
    }
}
