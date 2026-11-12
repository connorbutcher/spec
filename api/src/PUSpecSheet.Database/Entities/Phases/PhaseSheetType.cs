using PUSpecSheet.Database.Entities.SheetTypes;

namespace PUSpecSheet.Database.Entities.Phases;

public class PhaseSheetType
{
    public int PhaseId { get; set; }

    public Phase Phase { get; set; } = null!;

    public int SheetTypeId { get; set; }

    public SheetType SheetType { get; set; } = null!;
}
