namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// The key given to each item of one copy, by the item's id in that copy. Items that are the same item in different
/// copies share a key, and each key becomes one item row (one spec.SheetTableRows row, say).
/// </summary>
internal sealed class SnapshotKeys
{
    public Dictionary<int, int> Tables { get; } = [];

    public Dictionary<int, int> Groups { get; } = [];

    public Dictionary<int, int> Rows { get; } = [];
}
