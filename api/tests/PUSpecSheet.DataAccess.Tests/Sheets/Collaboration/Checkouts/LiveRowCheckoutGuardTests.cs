using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Collaboration.Checkouts;

public sealed class LiveRowCheckoutGuardTests
{
    private readonly LiveRowCheckoutTracker _tracker = new();
    private readonly SettableCurrentUser _currentUser = new() { UserId = 2 };
    private readonly LiveRowCheckoutGuard _guard;

    public LiveRowCheckoutGuardTests()
    {
        _guard = new LiveRowCheckoutGuard(_tracker, _currentUser);
        _tracker.TryCheckOut("sam", new LiveRowCheckout { SheetId = 1, RowId = 10, UserId = 1, DisplayName = "Sam" }, out _);
    }

    [Fact]
    public void ARowSomeoneElseIsIn_CannotBeChanged()
    {
        var refused = Assert.Throws<ConflictException>(() => _guard.EnsureNotHeldByOthers(10));

        Assert.Equal("This row is being edited by Sam.", refused.Message);
    }

    [Fact]
    public void MyOwnRow_AndAFreeRow_CanBeChanged()
    {
        _guard.EnsureNotHeldByOthers(11);

        _currentUser.UserId = 1;
        _guard.EnsureNotHeldByOthers(10);
    }
}
