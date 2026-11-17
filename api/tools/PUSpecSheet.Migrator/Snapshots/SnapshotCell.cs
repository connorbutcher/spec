namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// A cell of a row of a sheet snapshot, at one of its row template's cells, and what it holds.
/// </summary>
internal sealed record SnapshotCell
{
    public required int Id { get; init; }

    public required Guid ReferenceId { get; init; }

    public required int RowId { get; init; }

    public required int TableCellTemplateId { get; init; }

    public required CellContent Content { get; init; }
}
