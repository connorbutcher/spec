using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Domain.Values;

/// <summary>
/// A value entered in one sheet cell in one row revision. Each cell kind stores its values in its
/// own typed table in the "values" schema; a cell only ever has a value in the table for its kind.
/// </summary>
public interface ICellValue
{
    int Id { get; set; }

    int SheetRowRevisionId { get; set; }

    SheetRowRevision SheetRowRevision { get; set; }

    int SheetCellId { get; set; }

    SheetCell SheetCell { get; set; }
}
