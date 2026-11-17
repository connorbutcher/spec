namespace PUSpecSheet.Migrator.Legacy;

internal sealed record LegacyWorkflowActor
{
    public int Id { get; init; }

    public int? UserId { get; init; }

    public string? Comment { get; init; }

    public int State { get; init; }

    public DateTime? ActionedAtUtc { get; init; }
}
