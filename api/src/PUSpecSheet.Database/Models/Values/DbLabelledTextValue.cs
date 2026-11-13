using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Models.Values;

/// <summary>
/// One value of a labelled text cell: the text held in one of its slots. A cell has a record for each slot
/// that holds something, so each value can be looked up on its own.
/// </summary>
public class DbLabelledTextValue : IDbCellValue
{
    public int SheetRowRevisionId { get; set; }

    public DbSheetRowRevision SheetRowRevision { get; set; } = null!;

    public int SheetCellId { get; set; }

    public DbSheetCell SheetCell { get; set; } = null!;

    /// <summary>The slot's key, such as "rh".</summary>
    public string SlotKey { get; set; } = string.Empty;

    /// <summary>Where the slot comes in the cell, from 0, as of when the value was saved.</summary>
    public int Position { get; set; }

    public string Value { get; set; } = string.Empty;
}
