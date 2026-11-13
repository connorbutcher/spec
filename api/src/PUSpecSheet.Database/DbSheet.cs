namespace PUSpecSheet.Database;

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

    public DbPhase Phase { get; set; } = null!;

    public int SheetTypeId { get; set; }

    public DbSheetType SheetType { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Concurrency token. Every publish updates the sheet, so two people publishing at the same moment
    /// can't both claim the same version number; the second is rejected and retried.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    public ICollection<DbSheetTable> Tables { get; set; } = [];

    /// <summary>The numbered publishes of this sheet, oldest first.</summary>
    public ICollection<DbSheetVersion> Versions { get; set; } = [];
}
