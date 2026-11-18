namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// A table note as a copy shows it, which the history builder turns into a note item and a row on each table revision that has it.
/// </summary>
public record SnapshotNote
{
    public required string Note { get; init; }

    public required string SourceSystem { get; init; }

    public required bool IsObsolete { get; init; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }
}
