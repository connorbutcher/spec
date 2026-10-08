using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Collaboration;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

public sealed class LiveRowCheckoutGuardTests
{
    private readonly LiveRowCheckoutTracker tracker = new();
    private readonly SettableCurrentUser currentUser = new() { UserId = 2 };
    private readonly LiveRowCheckoutGuard guard;

    public LiveRowCheckoutGuardTests()
    {
        guard = new LiveRowCheckoutGuard(tracker, currentUser);
        tracker.TryCheckOut("sam", new LiveRowCheckout(1, 10, 1, "Sam"), out _);
    }

    [Fact]
    public void ARowSomeoneElseIsIn_CannotBeChanged()
    {
        var refused = Assert.Throws<ConflictException>(() => guard.EnsureNotHeldByOthers(10));

        Assert.Equal("This row is being edited by Sam.", refused.Message);
    }

    [Fact]
    public void MyOwnRow_AndAFreeRow_CanBeChanged()
    {
        guard.EnsureNotHeldByOthers(11);

        currentUser.UserId = 1;
        guard.EnsureNotHeldByOthers(10);
    }
}
