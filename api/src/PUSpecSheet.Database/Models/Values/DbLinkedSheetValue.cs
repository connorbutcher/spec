using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Models.Values;

// A cell pointing at a table, or an item in it, on another sheet. The target is held by public id, so it can be on a
// sheet that doesn't exist yet or has since been removed.
public class DbLinkedSheetValue : IDbCellValue
{
    public int SheetRowRevisionId { get; set; }

    public int SheetCellId { get; set; }

    public int? LinkedSheetTypeId { get; set; }

    public int? LinkedPhaseId { get; set; }

    public Guid? LinkedTablePublicId { get; set; }

    public Guid? LinkedItemPublicId { get; set; }

    public virtual DbSheetRowRevision SheetRowRevision { get; set; } = null!;

    public virtual DbSheetCell SheetCell { get; set; } = null!;
}
