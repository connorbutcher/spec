namespace PUSpecSheet.Query.Models;

public sealed record SlimPUSpecSheetType
{
    /// <summary>
    /// Gets the unique identifier of the PU Spec Sheet type.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the name of the PU Spec Sheet type. e.g. "Specification", "PFKs", "Parts".
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets the display order for the item.
    /// </summary>
    public required int DisplayOrder { get; init; }
}
