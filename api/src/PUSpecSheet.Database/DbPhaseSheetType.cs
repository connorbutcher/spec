namespace PUSpecSheet.Database;

public class DbPhaseSheetType
{
    public int PhaseId { get; set; }

    public DbPhase Phase { get; set; } = null!;

    public int SheetTypeId { get; set; }

    public DbSheetType SheetType { get; set; } = null!;
}
