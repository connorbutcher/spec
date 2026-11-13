using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.Database.Models.Phases;

public class DbPhaseSheetType
{
    public int PhaseId { get; set; }

    public int SheetTypeId { get; set; }

    public virtual DbPhase Phase { get; set; } = null!;

    public virtual DbSheetType SheetType { get; set; } = null!;
}
