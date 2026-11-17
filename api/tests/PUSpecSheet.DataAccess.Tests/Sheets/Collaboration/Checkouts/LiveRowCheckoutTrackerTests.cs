using PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Collaboration.Checkouts;

public sealed class LiveRowCheckoutTrackerTests
{
    private readonly LiveRowCheckoutTracker _tracker = new();

    [Fact]
    public void ARow_BelongsToWhoeverClickedIntoItFirst()
    {
        Assert.True(_tracker.TryCheckOut("sam", Row(10, user: 7), out _));
        Assert.False(_tracker.TryCheckOut("alex", Row(10, user: 8), out var heldBy));

        Assert.Equal(7, heldBy!.UserId);
        Assert.Equal(7, _tracker.HolderOf(10)!.UserId);
    }

    [Fact]
    public void ATab_HoldsOneRowAtATime()
    {
        _tracker.TryCheckOut("sam", Row(10, user: 7), out _);
        _tracker.TryCheckOut("sam", Row(11, user: 7), out _);

        Assert.Null(_tracker.HolderOf(10));
        Assert.Equal([11], _tracker.OnSheet(1).Select(checkout => checkout.RowId));
    }

    [Fact]
    public void TheSamePersonInAnotherTab_TakesTheRowWithThem()
    {
        _tracker.TryCheckOut("sam-tab-1", Row(10, user: 7), out _);

        Assert.True(_tracker.TryCheckOut("sam-tab-2", Row(10, user: 7), out _));

        // The first tab no longer holds it, so closing that tab frees nothing.
        Assert.Null(_tracker.Release("sam-tab-1"));
        Assert.Equal(7, _tracker.HolderOf(10)!.UserId);
    }

    [Fact]
    public void LeavingARow_FreesIt()
    {
        _tracker.TryCheckOut("sam", Row(10, user: 7), out _);

        Assert.Equal(10, _tracker.Release("sam")!.RowId);
        Assert.Null(_tracker.HolderOf(10));
        Assert.True(_tracker.TryCheckOut("alex", Row(10, user: 8), out _));
    }

    [Fact]
    public void ARowCanBeFreed_WhoeverIsInIt()
    {
        _tracker.TryCheckOut("sam", Row(10, user: 7), out _);

        Assert.Equal(7, _tracker.ReleaseRow(10)!.UserId);
        Assert.Null(_tracker.ReleaseRow(10));
        Assert.Null(_tracker.Release("sam"));
    }

    [Fact]
    public void ASheetLists_OnlyItsOwnRows()
    {
        _tracker.TryCheckOut("sam", Row(10, user: 7), out _);
        _tracker.TryCheckOut("alex", Row(20, user: 8, sheet: 2), out _);

        Assert.Equal([10], _tracker.OnSheet(1).Select(checkout => checkout.RowId));
        Assert.Equal("User 8", _tracker.OnSheet(2).Single().UserName);
    }

    private static LiveRowCheckout Row(int rowId, int user, int sheet = 1)
    {
        return new LiveRowCheckout { SheetId = sheet, RowId = rowId, UserId = user, DisplayName = $"User {user}" };
    }
}
