using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Users;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

/// <summary>
/// Stops a change to a row that someone else has clicked into. Their checkout is not a draft, so the
/// draft rules can't see it; anything that is about to change a row asks here first.
/// </summary>
public sealed class LiveRowCheckoutGuard
{
    private readonly LiveRowCheckoutTracker _tracker;
    private readonly ICurrentUser _currentUser;

    public LiveRowCheckoutGuard(LiveRowCheckoutTracker tracker, ICurrentUser currentUser)
    {
        _tracker = tracker;
        _currentUser = currentUser;
    }

    /// <exception cref="ConflictException">Someone other than the current user is in the row.</exception>
    public void EnsureNotHeldByOthers(int rowId)
    {
        if (_tracker.HolderOf(rowId) is { } holder && holder.UserId != _currentUser.UserId)
        {
            throw new ConflictException($"This row is being edited by {holder.DisplayName}.");
        }
    }
}
