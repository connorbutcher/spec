using PUSpecSheet.Domain.Phases;
using PUSpecSheet.Domain.SheetTypes;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// The PU Spec Sheet of one sheet type for one phase, e.g. V6's Parts sheet. Each sheet is versioned
/// independently of the phase's other sheets.
/// </summary>
public class Sheet
{
    public int Id { get; set; }

    public int PhaseId { get; set; }

    public Phase Phase { get; set; } = null!;

    public int SheetTypeId { get; set; }

    public SheetType SheetType { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<SheetTable> Tables { get; set; } = [];

    /// <summary>The numbered publishes of this sheet, oldest first.</summary>
    public ICollection<SheetVersion> Versions { get; set; } = [];
}
