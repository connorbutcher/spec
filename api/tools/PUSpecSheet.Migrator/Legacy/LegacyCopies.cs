namespace PUSpecSheet.Migrator.Legacy;

internal sealed record LegacyCopies
{
    public required IReadOnlyList<LegacyCopy> Copies { get; init; }

    public required IReadOnlyList<string> Warnings { get; init; }
}
