using PUSpecSheet.Abstractions.Sheets.Collaboration;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Collaboration.Takeovers;

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

    private readonly FakeRowCheckouts _checkouts = new();
    private readonly RecordingSheetLiveNotifier _notifier = new();
    private readonly SettableCurrentUser _currentUser = new() { UserId = Requester };
    private readonly SettableClock _clock = new();
    private readonly RowTakeoverStore _store = new();
    private readonly SheetPresenceTracker _presence = new();
    private readonly RowTakeoverOptions _options = new() { ResponseSeconds = 60 };
    private readonly RowTakeoversService _service;
    private readonly RowTakeoverSettler _settler;

    public RowTakeoverTests()
    {
        var closer = new RowTakeoverCloser(_checkouts, _notifier);
        _service = new RowTakeoversService(_checkouts, _currentUser, _store, _presence, closer, _notifier, _options, _clock);
        _settler = new RowTakeoverSettler(_store, closer, _clock);

        _checkouts.CheckOut(Row, Sheet, Holder);
        _presence.Join("holder-tab", new SheetConnection { SheetId = Sheet, UserId = Holder, DisplayName = "User 1" });
    }

    [Fact]
    public async Task AskingAHolderWhoIsThere_WaitsForTheirAnswer()
    {
        var takeover = await _service.RequestAsync(Row, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.Pending, takeover.Status);
        Assert.Equal("User 2", takeover.RequesterName);
        Assert.Equal("User 1", takeover.HolderName);
        Assert.Equal(TimeSpan.FromSeconds(60), takeover.ExpiresAtUtc - takeover.RequestedAtUtc);
        Assert.Equal(Holder, _checkouts.HolderOf(Row));
        Assert.Equal([RowTakeoverStatus.Pending], _notifier.Takeovers);
    }

    [Fact]
    public async Task AskingAHolderWhoIsAway_HandsTheRowOverAtOnce()
    {
        _presence.Leave("holder-tab");

        var takeover = await _service.RequestAsync(Row, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.GrantedHolderAway, takeover.Status);
        Assert.Equal(Requester, _checkouts.HolderOf(Row));
        Assert.Equal([Sheet], _notifier.ChangedSheets);
        Assert.Null(_store.ForRow(Row));
    }

    [Fact]
    public async Task AskingTwice_IsTheSameRequest()
    {
        var first = await _service.RequestAsync(Row, CancellationToken.None);
        var second = await _service.RequestAsync(Row, CancellationToken.None);

        Assert.Equal(first, second);
    }

    [Fact]
    public async Task ARowSomeoneElseHasAskedFor_CannotBeAskedForAgain()
    {
        await _service.RequestAsync(Row, CancellationToken.None);

        _currentUser.UserId = Bystander;
        var refused = await Assert.ThrowsAsync<ConflictException>(() => _service.RequestAsync(Row, CancellationToken.None));

        Assert.Contains("User 2 has already asked", refused.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ARowThatCannotBeTakenOver_SaysWhy()
    {
        _checkouts.AddRow(11);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.RequestAsync(99, CancellationToken.None));
        await Assert.ThrowsAsync<ConflictException>(() => _service.RequestAsync(11, CancellationToken.None));

        _currentUser.UserId = Holder;
        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.RequestAsync(Row, CancellationToken.None));
    }

    [Fact]
    public async Task ARowWithUnpublishedChanges_CannotBeAskedFor()
    {
        _checkouts.Change(Row);

        var refused = await Assert.ThrowsAsync<ConflictException>(() => _service.RequestAsync(Row, CancellationToken.None));

        Assert.Contains("User 1 has unpublished changes on this row", refused.Message, StringComparison.Ordinal);
        Assert.Null(_store.ForRow(Row));
        Assert.Empty(_notifier.Takeovers);
    }

    [Fact]
    public async Task ARowWithUnpublishedChanges_CannotBeTakenEvenWhenItsHolderIsAway()
    {
        _checkouts.Change(Row);
        _presence.Leave("holder-tab");

        await Assert.ThrowsAsync<ConflictException>(() => _service.RequestAsync(Row, CancellationToken.None));

        Assert.Equal(Holder, _checkouts.HolderOf(Row));
    }

    [Fact]
    public async Task Availability_GivesTheSameAnswerAsAsking()
    {
        Assert.Equal(new RowTakeoverAvailabilityDto
        {
            IsAvailable = true,
        }, await _service.GetAvailabilityAsync(Row, CancellationToken.None));

        _checkouts.Change(Row);
        var changed = await _service.GetAvailabilityAsync(Row, CancellationToken.None);
        Assert.False(changed.IsAvailable);
        Assert.Contains("unpublished changes", changed.Reason, StringComparison.Ordinal);

        _currentUser.UserId = Holder;
        Assert.False((await _service.GetAvailabilityAsync(Row, CancellationToken.None)).IsAvailable);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAvailabilityAsync(99, CancellationToken.None));
    }

    [Fact]
    public async Task Availability_SaysWhenSomeoneElseHasAlreadyAsked_ButNotToTheOneWhoDid()
    {
        await _service.RequestAsync(Row, CancellationToken.None);
        Assert.True((await _service.GetAvailabilityAsync(Row, CancellationToken.None)).IsAvailable);

        _currentUser.UserId = Bystander;
        var taken = await _service.GetAvailabilityAsync(Row, CancellationToken.None);

        Assert.False(taken.IsAvailable);
        Assert.Contains("User 2 has already asked", taken.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ChangingARowWhileARequestWaits_KeepsItWithTheHolder()
    {
        await _service.RequestAsync(Row, CancellationToken.None);

        _checkouts.Change(Row);
        await _settler.CloseBlockedAsync(Sheet, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.KeptForChanges, _notifier.Takeovers[^1]);
        Assert.Null(_store.ForRow(Row));
        Assert.Equal(Holder, _checkouts.HolderOf(Row));
    }

    [Fact]
    public async Task ARowChangedSinceItWasAskedFor_IsNotHandedOver_ByApprovalOrByTimeout()
    {
        var approvedLate = await _service.RequestAsync(Row, CancellationToken.None);
        _checkouts.Change(Row);

        _currentUser.UserId = Holder;
        var closed = await _service.ApproveAsync(approvedLate.Id, CancellationToken.None);
        Assert.Equal(RowTakeoverStatus.KeptForChanges, closed.Status);
        Assert.Equal(Holder, _checkouts.HolderOf(Row));

        // The same through the timeout: a second row, asked for while unchanged and changed before time ran out.
        _checkouts.CheckOut(11, Sheet, Holder);
        _currentUser.UserId = Requester;
        await _service.RequestAsync(11, CancellationToken.None);
        _checkouts.Change(11);
        _clock.Advance(TimeSpan.FromSeconds(60));
        await _settler.GrantOverdueAsync(CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.KeptForChanges, _notifier.Takeovers[^1]);
        Assert.Equal(Holder, _checkouts.HolderOf(11));
        Assert.Empty(_notifier.ChangedSheets);
    }

    [Fact]
    public async Task Approving_HandsTheRowOver_AndTellsEveryoneOnTheSheet()
    {
        var takeover = await _service.RequestAsync(Row, CancellationToken.None);

        _currentUser.UserId = Holder;
        var approved = await _service.ApproveAsync(takeover.Id, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.Approved, approved.Status);
        Assert.Equal(Requester, _checkouts.HolderOf(Row));
        Assert.Equal([RowTakeoverStatus.Pending, RowTakeoverStatus.Approved], _notifier.Takeovers);
        Assert.Equal([Sheet], _notifier.ChangedSheets);
    }

    [Fact]
    public async Task HandingOverARowItsHolderWasOnlyIn_DoesNotMakeEveryoneReadTheSheetAgain()
    {
        _checkouts.CheckOutLive(Row, Sheet, Holder);
        var takeover = await _service.RequestAsync(Row, CancellationToken.None);

        _currentUser.UserId = Holder;
        var approved = await _service.ApproveAsync(takeover.Id, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.Approved, approved.Status);
        Assert.Equal(Requester, _checkouts.HolderOf(Row));
        Assert.Empty(_notifier.ChangedSheets);
    }

    [Fact]
    public async Task Denying_KeepsTheRow()
    {
        var takeover = await _service.RequestAsync(Row, CancellationToken.None);

        _currentUser.UserId = Holder;
        var denied = await _service.DenyAsync(takeover.Id, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.Denied, denied.Status);
        Assert.Equal(Holder, _checkouts.HolderOf(Row));
        Assert.Empty(_notifier.ChangedSheets);
    }

    [Fact]
    public async Task OnlyTheHolderAnswers_AndOnlyTheRequesterWithdraws()
    {
        var takeover = await _service.RequestAsync(Row, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.ApproveAsync(takeover.Id, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.DenyAsync(takeover.Id, CancellationToken.None));

        _currentUser.UserId = Holder;
        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.CancelAsync(takeover.Id, CancellationToken.None));

        // Being refused leaves the request open for the right person.
        Assert.NotNull(_store.ForRow(Row));
    }

    [Fact]
    public async Task ARequest_IsSettledOnce()
    {
        var takeover = await _service.RequestAsync(Row, CancellationToken.None);
        await _service.CancelAsync(takeover.Id, CancellationToken.None);

        _currentUser.UserId = Holder;
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ApproveAsync(takeover.Id, CancellationToken.None));
        Assert.Equal(Holder, _checkouts.HolderOf(Row));
    }

    [Fact]
    public async Task ARequestNobodyAnswers_IsGrantedWhenItsTimeRunsOut()
    {
        await _service.RequestAsync(Row, CancellationToken.None);

        _clock.Advance(TimeSpan.FromSeconds(59));
        await _settler.GrantOverdueAsync(CancellationToken.None);
        Assert.Equal(Holder, _checkouts.HolderOf(Row));

        _clock.Advance(TimeSpan.FromSeconds(1));
        await _settler.GrantOverdueAsync(CancellationToken.None);

        Assert.Equal(Requester, _checkouts.HolderOf(Row));
        Assert.Equal(RowTakeoverStatus.GrantedOnTimeout, _notifier.Takeovers[^1]);
    }

    [Fact]
    public async Task ARowTheHolderFinishedWith_ClosesItsRequestAsReleased()
    {
        await _service.RequestAsync(Row, CancellationToken.None);

        _checkouts.Release(Row);
        await _settler.CloseBlockedAsync(Sheet, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.Released, _notifier.Takeovers[^1]);
        Assert.Null(_store.ForRow(Row));
    }

    [Fact]
    public async Task ARowStillHeld_KeepsItsRequestWaiting()
    {
        await _service.RequestAsync(Row, CancellationToken.None);

        await _settler.CloseBlockedAsync(Sheet, CancellationToken.None);

        Assert.NotNull(_store.ForRow(Row));
    }

    [Fact]
    public async Task ApprovingARowAlreadyReleased_ClosesAsReleased_NotAsAHandover()
    {
        var takeover = await _service.RequestAsync(Row, CancellationToken.None);
        _checkouts.Release(Row);

        _currentUser.UserId = Holder;
        var closed = await _service.ApproveAsync(takeover.Id, CancellationToken.None);

        Assert.Equal(RowTakeoverStatus.Released, closed.Status);
        Assert.Null(_checkouts.HolderOf(Row));
        Assert.Empty(_notifier.ChangedSheets);
    }
}
