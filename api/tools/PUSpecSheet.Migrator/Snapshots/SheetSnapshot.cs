namespace PUSpecSheet.Migrator.Snapshots;

internal sealed record SheetSnapshot
{
    public IReadOnlyList<SnapshotTable> Tables { get; init; } = [];

    public IReadOnlyList<SnapshotSection> Sections { get; init; } = [];

    public IReadOnlyList<SnapshotColumnBlock> ColumnBlocks { get; init; } = [];

    public IReadOnlyList<SnapshotRow> Rows { get; init; } = [];

    public IReadOnlyList<SnapshotCell> Cells { get; init; } = [];

    // By sheet type property id.
    public IReadOnlyDictionary<int, string?> Properties { get; init; } = new Dictionary<int, string?>();

    public IReadOnlyList<SnapshotLinkedTable> LinkedTables { get; init; } = [];

    // Null when the copy isn't checked against them: external references only exist for published versions.
    public IReadOnlyList<SnapshotExternalReference>? ExternalReferences { get; init; }

    // Built on first use. A snapshot isn't changed once made, so it never goes stale.
    private ILookup<int, SnapshotCell>? _cellsByRow;

    public IEnumerable<SnapshotCell> CellsOf(int rowId)
    {
        _cellsByRow ??= Cells.ToLookup(cell => cell.RowId);
        return _cellsByRow[rowId];
    }
}
