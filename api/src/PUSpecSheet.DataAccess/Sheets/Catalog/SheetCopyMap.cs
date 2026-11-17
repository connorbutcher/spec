using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Catalog;

public sealed class SheetCopyMap
{
    public Dictionary<int, DbSheetTable> Tables { get; } = [];

    public Dictionary<int, DbSheetColumnBlock> ColumnBlocks { get; } = [];

    public Dictionary<int, DbSheetSection> Sections { get; } = [];

    public Dictionary<int, DbSheetRow> Rows { get; } = [];

    public Dictionary<int, DbSheetRowRevision> RowRevisions { get; } = [];

    public Dictionary<int, DbSheetCell> Cells { get; } = [];
}
