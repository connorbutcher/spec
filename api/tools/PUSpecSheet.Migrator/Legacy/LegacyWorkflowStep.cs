namespace PUSpecSheet.Migrator.Legacy;

// One request for approval of a copy, and who was asked.
internal sealed record LegacyWorkflowStep
{
    public int Id { get; init; }

    public DateTime StartedAtUtc { get; init; }

    public int State { get; init; }

    public IReadOnlyList<LegacyWorkflowActor> Actors { get; init; } = [];
}
