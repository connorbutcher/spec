using PUSpecSheet.Query.Models.Values;

namespace PUSpecSheet.Query.Models.Properties;

public sealed record SlimPUSpecSheetPropertyValue
{
    /// <summary>
    /// Gets the unique identifier of the PU Spec Sheet property value.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the configuration info about the property, such as the name and display order.
    /// </summary>
    public required SlimPUSpecSheetPropertyInfo PropertyInfo { get; init; }

    /// <summary>
    /// Gets the value of the property.
    /// </summary>
    public required SlimValue Value { get; init; }
}
