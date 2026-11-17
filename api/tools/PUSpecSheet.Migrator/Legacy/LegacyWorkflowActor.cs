namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// One person asked to approve a copy (a WorkflowStepActors row), and what they did.
/// </summary>
internal sealed record LegacyWorkflowActor
{
    public required int Id { get; init; }

    public required int? UserId { get; init; }

    public required string? Comment { get; init; }

    public required int State { get; init; }

    public required DateTime? ActionedDateTime { get; init; }
}
