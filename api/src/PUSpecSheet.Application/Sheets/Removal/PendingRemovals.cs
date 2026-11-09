namespace PUSpecSheet.Application.Sheets.Removal;

/// <summary>
/// Decides whether a section or table someone is removing stops a change inside it. The person removing it
/// no longer sees it, but everyone else still does until the removal is published. A change made in there
/// meanwhile would be hidden with it once it is, stranded where nobody can see or publish it, so it is
/// refused.
/// </summary>
public static class PendingRemovals
{
    /// <summary>
    /// The removal that stops the user changing something inside, or null when none does. Another person's
    /// removal does; so does one waiting for approval, the user's own included.
    /// </summary>
    public static PendingRemoval? Blocking(IEnumerable<PendingRemoval> removals, int me)
    {
        return removals
            .Where(removal => removal.AwaitingApproval || removal.AuthorUserId != me)
            .OrderByDescending(removal => removal.Container)
            .FirstOrDefault();
    }

    /// <summary>The refusal for a change inside <paramref name="removal"/>, naming who is removing it.</summary>
    public static string RefusalFor(PendingRemoval removal, string authorName)
    {
        var container = removal.Container == RemovedContainer.Table ? "table" : "section";
        return removal.AwaitingApproval
            ? $"Removing the {container} this is in is waiting for approval, so it can't be changed until that is decided."
            : $"{authorName} is removing the {container} this is in, so it can't be changed unless they put it back.";
    }
}
