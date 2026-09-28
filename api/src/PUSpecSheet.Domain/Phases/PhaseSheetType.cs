using PUSpecSheet.Domain.SheetTypes;

namespace PUSpecSheet.Domain.Phases;

/// <summary>Makes a sheet type available to a phase.</summary>
public class PhaseSheetType
{
    public int PhaseId { get; set; }

    public Phase Phase { get; set; } = null!;

    public int SheetTypeId { get; set; }

    public SheetType SheetType { get; set; } = null!;
}
