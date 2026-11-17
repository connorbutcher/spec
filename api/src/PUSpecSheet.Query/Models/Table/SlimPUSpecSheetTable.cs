namespace PUSpecSheet.Query.Models.Table;

public record SlimPUSpecSheetTable
{
    /// <summary>
    /// Gets the unique identifier of the PU Spec Sheet.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the reference id of the PU Spec Sheet.
    /// The reference id of the PU Spec Sheet will remain the same across different versions of the same PU Spec Sheet, which can be used to identify the same PU Spec Sheet across different versions.
    /// </summary>
    public required Guid ReferenceId { get; init; }

    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the display order for the item.
    /// </summary>
    public required int Order { get; init; }

    /// <summary>
    /// Gets the list of groups for the table. Groups make a tree structure, where each group can have child groups. The groups with no parent group are the root groups, which are directly under the table.
    /// </summary>
    public required ICollection<SlimPUSpecSheetGroup> Groups { get; init; }

    /// <summary>
    /// Gets the list of notes for the table.
    /// </summary>
    public required ICollection<SlimPUSpecSheetTableNote> Notes { get; init; }
}
