using PUSpecSheet.Query.Models.Values;

namespace PUSpecSheet.Query.Models.Table.Cell;

public record SlimPUSpecSheetCell
{
    /// <summary>
    /// Gets the unique identifier of the PU Spec Sheet cell.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the reference id of the PU Spec Sheet cell.
    /// The reference id of the cell will remain the same across different versions of the same PU Spec Sheet, which can be used to identify the same cell across different versions.
    /// </summary>
    public required Guid ReferenceId { get; init; }

    /// <summary>
    /// Gets the column number of the cell in the table.
    /// </summary>
    public required int ColumnNumber { get; init; }

    /// <summary>
    /// Gets the column span of the cell.
    /// </summary>
    public required int ColSpan { get; init; }

    /// <summary>
    /// Gets the row span of the cell.
    /// </summary>
    public required int RowSpan { get; init; }

    /// <summary>
    /// Gets a value indicating whether the cell is an identifier cell. This IS NOT to be used as a key.
    /// </summary>
    public required bool IsIdentifier { get; init; }

    /// <summary>
    /// Gets the type of the cell, which indicates how to interpret the value of the cell.
    /// </summary>
    public required SlimPUSpecSheetTableCellType CellType { get; init; }

    /// <summary>
    /// Gets the value of the cell.
    /// </summary>
    public required SlimValue Value { get; init; }
}
