namespace PUSpecSheet.Query.Models.Properties;

public record SlimPUSpecSheetPropertyInfo
{
    /// <summary>
    /// Gets the unique identifier of the PU Spec Sheet property info.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the reference id of the property. The reference id of the property will remain the same across versions of the PU Spec Sheet, which can be used to identify the same property across different versions of the PU Spec Sheet.
    /// </summary>
    public required Guid ReferenceId { get; init; }

    /// <summary>
    /// Gets the name key of the property.
    /// </summary>
    public required string ValueKey { get; init; }

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public required string ValueName { get; init; }

    /// <summary>
    /// Gets the display order for the property.
    /// </summary>
    public required int DisplayOrder { get; init; }
}
