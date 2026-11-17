namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>The text in one slot of a labelled text cell.</summary>
internal sealed record LabelledSlot
{
    public required string SlotKey { get; init; }

    public required int Position { get; init; }

    public required string Value { get; init; }
}
