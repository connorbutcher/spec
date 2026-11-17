namespace PUSpecSheet.DataAccess.Sheets.Removal;

// Decides whether what people hold inside a section or table stops the current user removing it. Removing it would
// take their work with it, so anything held by someone else refuses, as does anything waiting for approval, including
// the current user's own. The current user's own open drafts go with it.
public static class HeldContents
{
    public static string? RefusalFor(
        string subject,
        IEnumerable<ContainedHold> holds,
        int me,
        IReadOnlyDictionary<int, string> names)
    {
        var blocking = holds
            .Where(hold => hold.AwaitingApproval || hold.UserId != me)
            .ToList();
        if (blocking.Count == 0)
        {
            return null;
        }

        if (blocking.Any(hold => hold.AwaitingApproval))
        {
            return $"Changes inside {subject} are waiting for approval, so it can't be removed until they are approved or rejected.";
        }

        var people = blocking
            .Select(hold => names.GetValueOrDefault(hold.UserId, "another user"))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        var verb = people.Count == 1 ? "is" : "are";
        var who = JoinNames(people);
        return $"{char.ToUpperInvariant(who[0])}{who[1..]} {verb} editing inside {subject}, so it can't be removed yet.";
    }

    private static string JoinNames(List<string> people)
    {
        if (people.Count == 1)
        {
            return people[0];
        }

        return $"{string.Join(", ", people.Take(people.Count - 1))} and {people[^1]}";
    }
}
