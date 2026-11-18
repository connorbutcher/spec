namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// Identifies a table note across copies, which don't keep a note's id: its table, who wrote it, when and its source
/// system, and its place among the table's notes that match on those.
/// </summary>
internal readonly record struct NoteKey
{
    public required int TableKey { get; init; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required string SourceSystem { get; init; }

    public required int Ordinal { get; init; }
}
