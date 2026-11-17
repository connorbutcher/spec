namespace PUSpecSheet.Query.Models.Table;

public record SlimPUSpecSheetGroup
{
    /// <summary>
    /// Gets the unique identifier of the PU Spec Sheet group.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the reference id of the PU Spec Sheet group.
    /// The reference id of the group will remain the same across different versions of the same PU Spec Sheet, which can be used to identify the same group across different versions.
    /// </summary>
    public required Guid ReferenceId { get; init; }


    /// <summary>
    /// Gets the identifier of the parent group associated with this entity, if any.
    /// </summary>
    /// <remarks>
    /// This property is optional and may be null if the entity does not belong to a parent group.
    /// </remarks>
    public required int? ParentGroupId { get; init; }

    /// <summary>
    /// Gets the type of the group.
    /// </summary>
    public required string GroupType { get; init; }

    /// <summary>
    /// Gets the order of the group.
    /// </summary>
    public required int Order { get; init; }

    /// <summary>
    /// Gets the rows of the group.
    /// </summary>
    /// <remarks>
    /// A group can have many or no rows. These are independent of the rows in the children groups. The rows in the group will be displayed above the children groups.
    /// </remarks>
    public required ICollection<SlimPUSpecSheetRow> Rows { get; init; }

    /// <summary>
    /// Gets the children groups of the group.
    /// The children groups will be displayed under the parent group and will be indented to show the hierarchy.
    /// </summary>
    public required ICollection<SlimPUSpecSheetGroup> Groups { get; init; }
}
