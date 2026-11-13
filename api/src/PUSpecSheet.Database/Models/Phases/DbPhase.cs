namespace PUSpecSheet.Database.Models.Phases;

/// <summary>
/// A build phase such as V6 or 01-A2. Phases form a tree through <see cref="ParentPhaseId"/>;
/// a phase with no parent sits at the top level.
/// </summary>
public class DbPhase
{
    public int Id { get; set; }

    /// <summary>The unique phase code, e.g. "V6" or "01-A2".</summary>
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Order among sibling phases.</summary>
    public int DisplayOrder { get; set; }

    public int? ParentPhaseId { get; set; }

    public virtual DbPhase? ParentPhase { get; set; }

    public virtual ICollection<DbPhase> ChildPhases { get; set; } = [];

    /// <summary>The sheet types selected as available to this phase. Not inherited from the parent.</summary>
    public virtual ICollection<DbPhaseSheetType> SheetTypes { get; set; } = [];
}
