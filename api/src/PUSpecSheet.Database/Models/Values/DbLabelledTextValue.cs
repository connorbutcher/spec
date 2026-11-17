using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Models.Values;

/// <summary>
/// One value of a labelled text cell: the text held in one of its slots. A cell has a record for each slot
/// that holds something, so each value can be looked up on its own.
/// </summary>
public class DbLabelledTextValue : IDbCellValue
{
    public int SheetRowRevisionId { get; set; }

    public int SheetCellId { get; set; }

    /// <summary>The slot's key, such as "rh".</summary>
    public required string SlotKey { get; set; }

    /// <summary>Where the slot comes in the cell, from 0, as of when the value was saved.</summary>
    public int Position { get; set; }

    public required string Value { get; set; }

    public virtual DbSheetRowRevision SheetRowRevision { get; set; } = null!;

    public virtual DbSheetCell SheetCell { get; set; } = null!;
}
