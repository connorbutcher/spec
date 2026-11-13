namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// A whole sheet as it stood at one moment: every table, section, column block, row and cell on it, with the
/// cells' contents. Items point at their parents by id. A legacy copy is read into this shape, and so is the
/// migrated sheet read back "as of" a version, so the two can be compared.
/// </summary>
internal sealed record SheetSnapshot
{
    public IReadOnlyList<SnapshotTable> Tables { get; init; } = [];

    public IReadOnlyList<SnapshotSection> Sections { get; init; } = [];

    public IReadOnlyList<SnapshotColumnBlock> ColumnBlocks { get; init; } = [];

    public IReadOnlyList<SnapshotRow> Rows { get; init; } = [];

    public IReadOnlyList<SnapshotCell> Cells { get; init; } = [];

    // Built on first use. A snapshot isn't changed once made, so it never goes stale.
    private ILookup<int, SnapshotCell>? cellsByRow;

    /// <summary>The cells of one row.</summary>
    public IEnumerable<SnapshotCell> CellsOf(int rowId)
    {
        cellsByRow ??= Cells.ToLookup(cell => cell.RowId);
        return cellsByRow[rowId];
    }
}
