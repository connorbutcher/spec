namespace PUSpecSheet.Query.Models.Structure;

public sealed record SlimStructureLevelValue
{
    /// <summary>
    /// Gets the unique identifier for the structure level value.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the reference identifier for the property associated with this structure level value, this is used to link the structure level value to a specific property in the specification sheet type.
    /// </summary>
    public required Guid PropertyReferenceId { get; init; }

    /// <summary>
    /// Gets the display order of the value in the structure.
    /// </summary>
    public required int DisplayOrder { get; init; }
}
