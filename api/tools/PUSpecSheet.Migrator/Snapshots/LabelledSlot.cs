namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>The text in one slot of a labelled text cell.</summary>
internal sealed record LabelledSlot(string SlotKey, int Position, string Value);
