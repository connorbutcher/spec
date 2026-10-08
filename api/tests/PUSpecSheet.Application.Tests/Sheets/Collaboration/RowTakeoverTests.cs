using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

/// <summary>
/// The takeover rules end to end, with the database and the browsers replaced by fakes: row 10 on sheet 1
/// is checked out to the holder, who has the sheet open and has not changed it, and the requester asks
/// for it.
/// </summary>
public sealed class RowTakeoverTests
{
    private const int Sheet = 1;
    private const int Row = 10;
    private const int Holder = 1;
    private const int Requester = 2;
    private const int Bystander = 3;

    private readonly FakeRowCheckouts checkouts = new();
    private readonly RecordingSheetLiveNotifier notifier = new();
    private readonly SettableCurrentUser currentUser = new() { UserId = Requester };
    private readonly SettableClock clock = new();
    private readonly RowTakeoverStore store = new();
    private readonly SheetPresenceTracker presence = new();
    private readonly RowTakeoverOptions options = new() { ResponseSeconds = 60 };
    private readonly RowTakeoverService service;
    private readonly RowTakeoverSettler settler;

    public RowTakeoverTests()
    {
        var closer = new RowTakeoverCloser(checkouts, notifier);
        service = new RowTakeoverService(checkouts, currentUser, store, presence, closer, notifier, options, clock);
        settler = new RowTakeoverSettler(store, closer, clock);

        checkouts.CheckOut(Row, Sheet, Holder);
        presence.Join("holder-tab", new SheetConnection(Sheet, Holder, "User 1"));
    }

    [Fact]
    public async Task AskingAHolderWhoIsThere_WaitsForTheirAnswer()
    {
        var takeover = await service.RequestAsync(Row, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.Pending, takeover.Status);
        Assert.Equal("User 2", takeover.RequesterName);
        Assert.Equal("User 1", takeover.HolderName);
        Assert.Equal(TimeSpan.FromSeconds(60), takeover.ExpiresAtUtc - takeover.RequestedAtUtc);
        Assert.Equal(Holder, checkouts.HolderOf(Row));
        Assert.Equal([RowTakeoverStatus.Pending], notifier.Takeovers);
    }

    [Fact]
    public async Task AskingAHolderWhoIsAway_HandsTheRowOverAtOnce()
    {
        presence.Leave("holder-tab");

        var takeover = await service.RequestAsync(Row, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.GrantedHolderAway, takeover.Status);
        Assert.Equal(Requester, checkouts.HolderOf(Row));
        Assert.Equal([Sheet], notifier.ChangedSheets);
        Assert.Null(store.ForRow(Row));
    }

    [Fact]
    public async Task AskingTwice_IsTheSameRequest()
    {
        var first = await service.RequestAsync(Row, CancellationToken.None);
        var second = await service.RequestAsync(Row, CancellationToken.None);

        Assert.Equal(first, second);
    }

    [Fact]
    public async Task ARowSomeoneElseHasAskedFor_CannotBeAskedForAgain()
    {
        await service.RequestAsync(Row, CancellationToken.None);

        currentUser.UserId = Bystander;
        var refused = await Assert.ThrowsAsync<ConflictException>(() => service.RequestAsync(Row, CancellationToken.None));

        Assert.Contains("User 2 has already asked", refused.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ARowThatCannotBeTakenOver_SaysWhy()
    {
        checkouts.AddRow(11);

        await Assert.ThrowsAsync<NotFoundException>(() => service.RequestAsync(99, CancellationToken.None));
        await Assert.ThrowsAsync<ConflictException>(() => service.RequestAsync(11, CancellationToken.None));

        currentUser.UserId = Holder;
        await Assert.ThrowsAsync<InvalidRequestException>(() => service.RequestAsync(Row, CancellationToken.None));
    }

    [Fact]
    public async Task ARowWithUnpublishedChanges_CannotBeAskedFor()
    {
        checkouts.Change(Row);

        var refused = await Assert.ThrowsAsync<ConflictException>(() => service.RequestAsync(Row, CancellationToken.None));

        Assert.Contains("User 1 has unpublished changes on this row", refused.Message, StringComparison.Ordinal);
        Assert.Null(store.ForRow(Row));
        Assert.Empty(notifier.Takeovers);
    }

    [Fact]
    public async Task ARowWithUnpublishedChanges_CannotBeTakenEvenWhenItsHolderIsAway()
    {
        checkouts.Change(Row);
        presence.Leave("holder-tab");

        await Assert.ThrowsAsync<ConflictException>(() => service.RequestAsync(Row, CancellationToken.None));

        Assert.Equal(Holder, checkouts.HolderOf(Row));
    }

    [Fact]
    public async Task Availability_GivesTheSameAnswerAsAsking()
    {
        Assert.Equal(new RowTakeoverAvailabilityDto(true, null), await service.GetAvailabilityAsync(Row, CancellationToken.None));

        checkouts.Change(Row);
        var changed = await service.GetAvailabilityAsync(Row, CancellationToken.None);
        Assert.False(changed.IsAvailable);
        Assert.Contains("unpublished changes", changed.Reason, StringComparison.Ordinal);

        currentUser.UserId = Holder;
        Assert.False((await service.GetAvailabilityAsync(Row, CancellationToken.None)).IsAvailable);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAvailabilityAsync(99, CancellationToken.None));
    }

    [Fact]
    public async Task Availability_SaysWhenSomeoneElseHasAlreadyAsked_ButNotToTheOneWhoDid()
    {
        await service.RequestAsync(Row, CancellationToken.None);
        Assert.True((await service.GetAvailabilityAsync(Row, CancellationToken.None)).IsAvailable);

        currentUser.UserId = Bystander;
        var taken = await service.GetAvailabilityAsync(Row, CancellationToken.None);

        Assert.False(taken.IsAvailable);
        Assert.Contains("User 2 has already asked", taken.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ChangingARowWhileARequestWaits_KeepsItWithTheHolder()
    {
        await service.RequestAsync(Row, CancellationToken.None);

        checkouts.Change(Row);
        await settler.CloseBlockedAsync(Sheet, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.KeptForChanges, notifier.Takeovers[^1]);
        Assert.Null(store.ForRow(Row));
        Assert.Equal(Holder, checkouts.HolderOf(Row));
    }

    [Fact]
    public async Task ARowChangedSinceItWasAskedFor_IsNotHandedOver_ByApprovalOrByTimeout()
    {
        var approvedLate = await service.RequestAsync(Row, CancellationToken.None);
        checkouts.Change(Row);

        currentUser.UserId = Holder;
        var closed = await service.ApproveAsync(approvedLate.Id, CancellationToken.None);
        Assert.Equal(RowTakeoverStatus.KeptForChanges, closed.Status);
        Assert.Equal(Holder, checkouts.HolderOf(Row));

        // The same through the timeout: a second row, asked for while unchanged and changed before time ran out.
        checkouts.CheckOut(11, Sheet, Holder);
        currentUser.UserId = Requester;
        await service.RequestAsync(11, CancellationToken.None);
        checkouts.Change(11);
        clock.Advance(TimeSpan.FromSeconds(60));
        await settler.GrantOverdueAsync(CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.KeptForChanges, notifier.Takeovers[^1]);
        Assert.Equal(Holder, checkouts.HolderOf(11));
        Assert.Empty(notifier.ChangedSheets);
    }

    [Fact]
    public async Task Approving_HandsTheRowOver_AndTellsEveryoneOnTheSheet()
    {
        var takeover = await service.RequestAsync(Row, CancellationToken.None);

        currentUser.UserId = Holder;
        var approved = await service.ApproveAsync(takeover.Id, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.Approved, approved.Status);
        Assert.Equal(Requester, checkouts.HolderOf(Row));
        Assert.Equal([RowTakeoverStatus.Pending, RowTakeoverStatus.Approved], notifier.Takeovers);
        Assert.Equal([Sheet], notifier.ChangedSheets);
    }

    [Fact]
    public async Task Denying_KeepsTheRow()
    {
        var takeover = await service.RequestAsync(Row, CancellationToken.None);

        currentUser.UserId = Holder;
        var denied = await service.DenyAsync(takeover.Id, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.Denied, denied.Status);
        Assert.Equal(Holder, checkouts.HolderOf(Row));
        Assert.Empty(notifier.ChangedSheets);
    }

    [Fact]
    public async Task OnlyTheHolderAnswers_AndOnlyTheRequesterWithdraws()
    {
        var takeover = await service.RequestAsync(Row, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidRequestException>(() => service.ApproveAsync(takeover.Id, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidRequestException>(() => service.DenyAsync(takeover.Id, CancellationToken.None));

        currentUser.UserId = Holder;
        await Assert.ThrowsAsync<InvalidRequestException>(() => service.CancelAsync(takeover.Id, CancellationToken.None));

        // Being refused leaves the request open for the right person.
        Assert.NotNull(store.ForRow(Row));
    }

    [Fact]
    public async Task ARequest_IsSettledOnce()
    {
        var takeover = await service.RequestAsync(Row, CancellationToken.None);
        await service.CancelAsync(takeover.Id, CancellationToken.None);

        currentUser.UserId = Holder;
        await Assert.ThrowsAsync<NotFoundException>(() => service.ApproveAsync(takeover.Id, CancellationToken.None));
        Assert.Equal(Holder, checkouts.HolderOf(Row));
    }

    [Fact]
    public async Task ARequestNobodyAnswers_IsGrantedWhenItsTimeRunsOut()
    {
        await service.RequestAsync(Row, CancellationToken.None);

        clock.Advance(TimeSpan.FromSeconds(59));
        await settler.GrantOverdueAsync(CancellationToken.None);
        Assert.Equal(Holder, checkouts.HolderOf(Row));

        clock.Advance(TimeSpan.FromSeconds(1));
        await settler.GrantOverdueAsync(CancellationToken.None);

        Assert.Equal(Requester, checkouts.HolderOf(Row));
        Assert.Equal(RowTakeoverStatus.GrantedOnTimeout, notifier.Takeovers[^1]);
    }

    [Fact]
    public async Task ARowTheHolderFinishedWith_ClosesItsRequestAsReleased()
    {
        await service.RequestAsync(Row, CancellationToken.None);

        checkouts.Release(Row);
        await settler.CloseBlockedAsync(Sheet, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.Released, notifier.Takeovers[^1]);
        Assert.Null(store.ForRow(Row));
    }

    [Fact]
    public async Task ARowStillHeld_KeepsItsRequestWaiting()
    {
        await service.RequestAsync(Row, CancellationToken.None);

        await settler.CloseBlockedAsync(Sheet, CancellationToken.None);

        Assert.NotNull(store.ForRow(Row));
    }

    [Fact]
    public async Task ApprovingARowAlreadyReleased_ClosesAsReleased_NotAsAHandover()
    {
        var takeover = await service.RequestAsync(Row, CancellationToken.None);
        checkouts.Release(Row);

        currentUser.UserId = Holder;
        var closed = await service.ApproveAsync(takeover.Id, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.Released, closed.Status);
        Assert.Null(checkouts.HolderOf(Row));
        Assert.Empty(notifier.ChangedSheets);
    }
}
