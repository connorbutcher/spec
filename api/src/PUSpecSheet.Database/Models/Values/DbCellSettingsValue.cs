using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Database.Models.Values;

/// <summary>
/// The settings chosen on the sheet for one cell, in one row revision. Kept beside the cell's value so
/// they are versioned with the row in the same way. A cell with nothing chosen has no record.
/// </summary>
public class DbCellSettingsValue : IDbCellValue
{
    public int SheetRowRevisionId { get; set; }

    public int SheetCellId { get; set; }

    public CellInstanceSettings Settings { get; set; } = null!;

    public virtual DbSheetRowRevision SheetRowRevision { get; set; } = null!;

    public virtual DbSheetCell SheetCell { get; set; } = null!;
}
