using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Models.Values;

/// <summary>
/// A value entered in one sheet cell in one row revision. Each cell kind stores its values in its
/// own typed table in the "cell" schema; a cell only ever has a value in the table for its kind.
/// </summary>
public interface IDbCellValue
{
    int SheetRowRevisionId { get; set; }

    int SheetCellId { get; set; }

    DbSheetRowRevision SheetRowRevision { get; set; }

    DbSheetCell SheetCell { get; set; }
}
