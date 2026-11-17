using PUSpecSheet.Query.Types;

namespace PUSpecSheet.Query.Requests;

public record ExternalSystemReferenceRequest
{
    /// <summary>
    /// Gets the unique identifier for the specification sheet associated the external reference.
    /// </summary>
    public required int PUSpecSheetId { get; init; }

    /// <summary>
    /// Gets the external identifier associated with the external reference.
    /// </summary>    
    public required int ExternalId { get; init; }

    /// <summary>
    /// Gets the reference id of the spec sheet item associated with the external reference. This is used to correlate the reference to a specific item on the spec sheet, such as a property or table.
    /// </summary>    
    public required Guid ReferenceId { get; init; }

    /// <summary>
    /// Gets the name of the external system.
    /// </summary>    
    public required string SystemName { get; init; }

    /// <summary>
    /// Gets the name of the owner associated with the external system reference.
    /// </summary>
    public required string OwnerName { get; init; }

    /// <summary>
    /// Gets the URL that points to the external system reference. This URL should direct users to the relevant page or resource in the external system that corresponds to the reference on the specification sheet.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Gets the type of the specification sheet reference.
    /// </summary>
    public required SpecSheetExternalReferenceType Type { get; init; }

    /// <summary>
    /// Gets the grouping string. This is used in the PU Spec Sheet UI to group external references together based on the value of this property. For example, if multiple external references have the same grouping value, they can be displayed together in the UI under a common section or heading. The grouping property allows for better organization and categorization of external references on the specification sheet.
    /// </summary>
    public string? Grouping { get; init; }


    /// <summary>
    /// Gets the system icon URL. This is displayed in the PU Spec Sheet UI as an icon representing the external system associated with the reference. The URL should point to a hosted image that can be used as an icon.
    /// </summary>
    public string? SystemIconUrl { get; init; }

    /// <summary>
    /// Gets the description of the external reference. This is displayed in the PU Spec Sheet UI as additional information about the external reference, providing context or details to users about the reference and its relevance to the specification sheet item it is associated with.
    /// </summary>
    public string? Description { get; init; }
}
