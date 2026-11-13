namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// The identity given to each item of one copy, by the item's id in that copy. Items that are the same item in
/// different copies share a key, and each key becomes one identity row (one <c>DbSheetRow</c>, say).
/// </summary>
internal sealed class SnapshotKeys
{
    public Dictionary<int, int> Tables { get; } = [];

    public Dictionary<int, int> Sections { get; } = [];

    public Dictionary<int, int> ColumnBlocks { get; } = [];

    public Dictionary<int, int> Rows { get; } = [];
}
