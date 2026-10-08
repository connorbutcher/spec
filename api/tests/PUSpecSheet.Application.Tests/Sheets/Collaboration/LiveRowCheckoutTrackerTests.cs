using PUSpecSheet.Application.Sheets.Collaboration;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

public sealed class LiveRowCheckoutTrackerTests
{
    private readonly LiveRowCheckoutTracker tracker = new();

    [Fact]
    public void ARow_BelongsToWhoeverClickedIntoItFirst()
    {
        Assert.True(tracker.TryCheckOut("sam", Row(10, user: 7), out _));
        Assert.False(tracker.TryCheckOut("alex", Row(10, user: 8), out var heldBy));

        Assert.Equal(7, heldBy!.UserId);
        Assert.Equal(7, tracker.HolderOf(10)!.UserId);
    }

    [Fact]
    public void ATab_HoldsOneRowAtATime()
    {
        tracker.TryCheckOut("sam", Row(10, user: 7), out _);
        tracker.TryCheckOut("sam", Row(11, user: 7), out _);

        Assert.Null(tracker.HolderOf(10));
        Assert.Equal([11], tracker.OnSheet(1).Select(checkout => checkout.RowId));
    }

    [Fact]
    public void TheSamePersonInAnotherTab_TakesTheRowWithThem()
    {
        tracker.TryCheckOut("sam-tab-1", Row(10, user: 7), out _);

        Assert.True(tracker.TryCheckOut("sam-tab-2", Row(10, user: 7), out _));

        // The first tab no longer holds it, so closing that tab frees nothing.
        Assert.Null(tracker.Release("sam-tab-1"));
        Assert.Equal(7, tracker.HolderOf(10)!.UserId);
    }

    [Fact]
    public void LeavingARow_FreesIt()
    {
        tracker.TryCheckOut("sam", Row(10, user: 7), out _);

        Assert.Equal(10, tracker.Release("sam")!.RowId);
        Assert.Null(tracker.HolderOf(10));
        Assert.True(tracker.TryCheckOut("alex", Row(10, user: 8), out _));
    }

    [Fact]
    public void ARowCanBeFreed_WhoeverIsInIt()
    {
        tracker.TryCheckOut("sam", Row(10, user: 7), out _);

        Assert.Equal(7, tracker.ReleaseRow(10)!.UserId);
        Assert.Null(tracker.ReleaseRow(10));
        Assert.Null(tracker.Release("sam"));
    }

    [Fact]
    public void ASheetLists_OnlyItsOwnRows()
    {
        tracker.TryCheckOut("sam", Row(10, user: 7), out _);
        tracker.TryCheckOut("alex", Row(20, user: 8, sheet: 2), out _);

        Assert.Equal([10], tracker.OnSheet(1).Select(checkout => checkout.RowId));
        Assert.Equal("User 8", tracker.OnSheet(2).Single().UserName);
    }

    private static LiveRowCheckout Row(int rowId, int user, int sheet = 1)
    {
        return new LiveRowCheckout(sheet, rowId, user, $"User {user}");
    }
}
