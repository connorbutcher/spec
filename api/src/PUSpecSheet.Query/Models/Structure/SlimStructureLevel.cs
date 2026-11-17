namespace PUSpecSheet.Query.Models.Structure;

public sealed record SlimStructureLevel
{
    /// <summary>
    /// Gets the unique identifier for the structure level.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the sort orientation for the structure level, which determines the order in which it is displayed or processed.
    /// </summary>
    public required int SortOrientation { get; init; }

    /// <summary>
    /// Gets the collection of level values associated with this structure level, this represents the properties in the structure level and their display order.
    /// </summary>
    public required IEnumerable<SlimStructureLevelValue> LevelValues { get; init; }
}
