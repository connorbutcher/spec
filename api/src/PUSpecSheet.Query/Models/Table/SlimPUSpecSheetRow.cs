using PUSpecSheet.Query.Models.Table.Cell;

namespace PUSpecSheet.Query.Models.Table;

public record SlimPUSpecSheetRow
{
    /// <summary>
    /// Gets the unique identifier of the PU Spec Sheet row.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the reference id of the PU Spec Sheet row.
    /// </summary>
    public required Guid ReferenceId { get; init; }

    /// <summary>
    /// Gets the display order for the row.
    /// </summary>
    public required int Order { get; init; }

    /// <summary>
    /// Gets the list of cells in the row.
    /// </summary>
    public required ICollection<SlimPUSpecSheetCell> Cells { get; init; }
}
