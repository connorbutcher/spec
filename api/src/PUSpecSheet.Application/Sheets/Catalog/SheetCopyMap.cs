using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Application.Sheets.Catalog;

/// <summary>What each item of the sheet being copied became on the new sheet, by the source item's id.</summary>
public sealed class SheetCopyMap
{
    public Dictionary<int, DbSheetTable> Tables { get; } = [];

    public Dictionary<int, DbSheetColumnBlock> ColumnBlocks { get; } = [];

    public Dictionary<int, DbSheetSection> Sections { get; } = [];

    public Dictionary<int, DbSheetRow> Rows { get; } = [];

    /// <summary>The new row revisions, by the source row revision's id.</summary>
    public Dictionary<int, DbSheetRowRevision> RowRevisions { get; } = [];

    public Dictionary<int, DbSheetCell> Cells { get; } = [];
}
