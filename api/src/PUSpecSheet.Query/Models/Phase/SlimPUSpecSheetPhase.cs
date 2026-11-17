namespace PUSpecSheet.Query.Models.Phase;

public record SlimPUSpecSheetPhase
{
    /// <summary>
    /// Gets the unique identifier of the PU spec sheet phase.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the unique identifier of the parent phase. Null if this is a root phase.
    /// </summary>
    public required int? ParentSpecSheetPhaseId { get; init; }

    /// <summary>
    /// Gets the name of the PU Spec Sheet phase. e.g. "V6", "SC", "01-A4".
    /// </summary>
    public required string Phase { get; init; }

    /// <summary>
    /// Gets whether the phase is obsolete.    
    /// </summary>
    /// <remarks>
    /// If a phase is marked as obsolete, it means that the phase is no longer in use and should not be used for new PU spec sheets.
    /// However, existing PU spec sheets that are already using the obsolete phase will not be affected and can still be accessed and modified.
    /// </remarks>
    public required bool IsObsolete { get; init; }

    /// <summary>
    /// Gets the display order for the item.
    /// </summary>
    public required int DisplayOrder { get; init; }
}
