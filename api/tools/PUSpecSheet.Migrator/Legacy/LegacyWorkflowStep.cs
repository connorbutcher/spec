namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// One request for approval of a copy (a WorkflowSteps row), and who was asked.
/// </summary>
internal sealed record LegacyWorkflowStep
{
    public required int Id { get; init; }

    public required DateTime StartedDateTime { get; init; }

    public required int State { get; init; }

    public required IReadOnlyList<LegacyWorkflowActor> Actors { get; init; }
}
