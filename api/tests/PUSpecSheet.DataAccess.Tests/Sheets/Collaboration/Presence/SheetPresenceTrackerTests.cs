using PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Collaboration.Presence;

public sealed class SheetPresenceTrackerTests
{
    [Fact]
    public void SomeoneWithTwoTabsOpen_IsListedOnce()
    {
        var tracker = new SheetPresenceTracker();
        tracker.Join("a", new SheetConnection { SheetId = 1, UserId = 7, DisplayName = "Sam" });
        tracker.Join("b", new SheetConnection { SheetId = 1, UserId = 7, DisplayName = "Sam" });
        tracker.Join("c", new SheetConnection { SheetId = 1, UserId = 8, DisplayName = "Alex" });

        var users = tracker.UsersOn(1);

        Assert.Equal(["Alex", "Sam"], users.Select(user => user.DisplayName));
        Assert.Equal(2, users.Single(user => user.UserId == 7).ConnectionCount);
    }

    [Fact]
    public void PeopleOnAnotherSheet_AreNotListed()
    {
        var tracker = new SheetPresenceTracker();
        tracker.Join("a", new SheetConnection { SheetId = 1, UserId = 7, DisplayName = "Sam" });
        tracker.Join("b", new SheetConnection { SheetId = 2, UserId = 8, DisplayName = "Alex" });

        Assert.Equal([7], tracker.UsersOn(1).Select(user => user.UserId));
        Assert.False(tracker.IsPresent(1, 8));
    }

    [Fact]
    public void SomeoneIsPresent_UntilTheirLastTabCloses()
    {
        var tracker = new SheetPresenceTracker();
        tracker.Join("a", new SheetConnection { SheetId = 1, UserId = 7, DisplayName = "Sam" });
        tracker.Join("b", new SheetConnection { SheetId = 1, UserId = 7, DisplayName = "Sam" });

        Assert.Equal(new SheetConnection { SheetId = 1, UserId = 7, DisplayName = "Sam" }, tracker.Leave("a"));
        Assert.True(tracker.IsPresent(1, 7));

        tracker.Leave("b");
        Assert.False(tracker.IsPresent(1, 7));
        Assert.Null(tracker.Leave("b"));
    }
}
