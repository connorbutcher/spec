namespace PUSpecSheet.Migrator.Legacy;

/// <summary>
/// Every copy of one sheet, and what reading them found that the revisions can't keep.
/// </summary>
internal sealed record LegacyCopies
{
    public required IReadOnlyList<LegacyCopy> Copies { get; init; }

    public required IReadOnlyList<string> Warnings { get; init; }
}
