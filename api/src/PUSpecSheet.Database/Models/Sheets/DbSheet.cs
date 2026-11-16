using PUSpecSheet.Database.Models.Phases;
using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.Database.Models.Sheets;

/// <summary>
/// The PU Spec Sheet of one sheet type for one phase, e.g. V6's Parts sheet. Each sheet is versioned
/// independently of the phase's other sheets.
/// </summary>
public class DbSheet
{
    public int Id { get; set; }

    /// <summary>Stable identifier that never changes, for finding the sheet from anywhere.</summary>
    public Guid PublicId { get; set; }

    public int PhaseId { get; set; }

    public int SheetTypeId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Concurrency token. Every publish updates the sheet, so two people publishing at the same moment
    /// can't both claim the same version number; the second is rejected and retried.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    public virtual DbPhase Phase { get; set; } = null!;

    public virtual DbSheetType SheetType { get; set; } = null!;

    public virtual ICollection<DbSheetTable> Tables { get; set; } = [];

    /// <summary>Values about the sheet as a whole, versioned with it.</summary>
    public virtual ICollection<DbSheetProperty> Properties { get; set; } = [];

    /// <summary>References to the sheet pushed in by external systems, each revision tied to a version.</summary>
    public virtual ICollection<DbSheetExternalReference> ExternalReferences { get; set; } = [];

    /// <summary>The numbered publishes of this sheet, oldest first.</summary>
    public virtual ICollection<DbSheetVersion> Versions { get; set; } = [];
}
