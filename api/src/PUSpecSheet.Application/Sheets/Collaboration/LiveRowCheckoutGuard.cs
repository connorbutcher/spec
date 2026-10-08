using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Users;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// Stops a change to a row that someone else has clicked into. Their checkout is not a draft, so the
/// draft rules can't see it; anything that is about to change a row asks here first.
/// </summary>
public sealed class LiveRowCheckoutGuard(LiveRowCheckoutTracker tracker, ICurrentUser currentUser)
{
    /// <exception cref="ConflictException">Someone other than the current user is in the row.</exception>
    public void EnsureNotHeldByOthers(int rowId)
    {
        if (tracker.HolderOf(rowId) is { } holder && holder.UserId != currentUser.UserId)
        {
            throw new ConflictException($"This row is being edited by {holder.DisplayName}.");
        }
    }
}
