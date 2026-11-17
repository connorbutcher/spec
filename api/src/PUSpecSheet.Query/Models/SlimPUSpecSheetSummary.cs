using PUSpecSheet.Query.Models.Phase;
using PUSpecSheet.Query.Models.Properties;
using PUSpecSheet.Query.Models.Structure;

namespace PUSpecSheet.Query.Models;

public sealed record SlimPUSpecSheetSummary
{
    /// <summary>
    /// Gets the unique identifier of the PU spec sheet.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the identifier for the legacy specification sheet associated with this entity.
    /// </summary>
    /// <remarks>This property is required for compatibility with legacy systems or documentation. A value of
    /// <see langword="null"/> indicates that no legacy specification sheet is linked.</remarks>
    public required int? LegacySpecSheetId { get; init; }

    /// <summary>
    /// Get the phase of the PU Spec Sheet. e.g. "V6", "SC", "01-A4".
    /// </summary>
    public required SlimPUSpecSheetPhase PUSpecSheetPhase { get; init; }

    /// <summary>
    /// Gets the type of PU Spec Sheet. e.g. "Specification", "PFKs", "Parts".
    /// </summary>
    public required SlimPUSpecSheetType PUSpecSheetType { get; init; }

    /// <summary>
    /// Gets the date and time when this version of the PU Spec Sheet was published.
    /// </summary>
    public required DateTime PublishedDateTime { get; init; }

    /// <summary>
    /// Gets what's changed in this version of the PU Spec Sheet.
    /// </summary>
    public required string ChangeLog { get; init; }

    /// <summary>
    /// Gets the properties in the PU Spec Sheet.
    /// </summary>
    public required ICollection<SlimPUSpecSheetPropertyValueSummary> Properties { get; init; }

    /// <summary>
    /// Gets the row level structure to display the spec sheet as.
    /// </summary>
    public required SlimStructureLevel DisplayLevel { get; init; }
}
