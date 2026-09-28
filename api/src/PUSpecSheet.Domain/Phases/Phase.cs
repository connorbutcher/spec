namespace PUSpecSheet.Domain.Phases;

/// <summary>
/// A build phase such as V6 or 01-A2. Phases form a tree through <see cref="ParentPhaseId"/>;
/// a phase with no parent sits at the top level.
/// </summary>
public class Phase
{
    public int Id { get; set; }

    /// <summary>The unique phase code, e.g. "V6" or "01-A2".</summary>
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Order among sibling phases.</summary>
    public int DisplayOrder { get; set; }

    public int? ParentPhaseId { get; set; }

    public Phase? ParentPhase { get; set; }

    public ICollection<Phase> ChildPhases { get; set; } = [];

    /// <summary>The sheet types selected as available to this phase. Not inherited from the parent.</summary>
    public ICollection<PhaseSheetType> SheetTypes { get; set; } = [];
}
