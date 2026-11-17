namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// What a sheet showed at one point: one legacy copy, or the migrated sheet read back as of a version (or with its
/// drafts). Both are read into this one shape so that they can be compared.
/// </summary>
internal sealed record SheetSnapshot
{
    public required IReadOnlyList<SnapshotTable> Tables { get; init; }

    public required IReadOnlyList<SnapshotGroup> Groups { get; init; }

    public required IReadOnlyList<SnapshotRow> Rows { get; init; }

    public required IReadOnlyList<SnapshotCell> Cells { get; init; }

    /// <summary>
    /// The sheet's property values, by SpecSheetTypePropertyID.
    /// </summary>
    public required IReadOnlyDictionary<int, string?> Properties { get; init; }

    public required IReadOnlyList<SnapshotLinkedTable> LinkedTables { get; init; }

    /// <summary>
    /// Null when the snapshot isn't checked against them: external references only belong to published versions.
    /// </summary>
    public required IReadOnlyList<SnapshotExternalReference>? ExternalReferences { get; init; }

    // Built on first use, and again if a copy made with "with" was given other cells.
    private IReadOnlyList<SnapshotCell>? _indexedCells;
    private ILookup<int, SnapshotCell>? _cellsByRow;

    public IEnumerable<SnapshotCell> CellsOf(int rowId)
    {
        if (_cellsByRow is null || !ReferenceEquals(_indexedCells, Cells))
        {
            _cellsByRow = Cells.ToLookup(cell => cell.RowId);
            _indexedCells = Cells;
        }

        return _cellsByRow[rowId];
    }
}
