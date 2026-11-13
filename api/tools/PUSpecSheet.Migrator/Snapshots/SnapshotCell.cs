namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// A cell of a row on a sheet snapshot and what it holds. <see cref="ColumnBlockId"/> is the column block copy
/// the cell belongs to, or null for the row's own cells.
/// </summary>
internal sealed record SnapshotCell
{
    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int RowId { get; init; }

    public int TemplateCellId { get; init; }

    public int? ColumnBlockId { get; init; }

    public CellContent Content { get; init; } = CellContent.Empty;
}
