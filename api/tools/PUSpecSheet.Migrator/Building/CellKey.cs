namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// Identifies a cell across copies: its row and its template cell.
/// </summary>
internal readonly record struct CellKey
{
    public required int RowKey { get; init; }

    public required int TableCellTemplateId { get; init; }
}
