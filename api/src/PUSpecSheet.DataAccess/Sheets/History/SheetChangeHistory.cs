namespace PUSpecSheet.DataAccess.Sheets.History;

// When things on a sheet last changed, taken from the published revisions up to the moment being viewed. A cell
// changes when its value differs from the previous revision's (or the row was added with a value); a row changes when
// any cell does or when it is added; a section changes when a section or row directly under it is added, removed or
// moved. Once built it is only read, so one instance can serve many requests.
public sealed class SheetChangeHistory
{
    public static SheetChangeHistory Empty { get; } = new();

    public Dictionary<int, SheetChangeRecord> Cells { get; } = [];

    public Dictionary<int, SheetChangeRecord> Rows { get; } = [];

    public Dictionary<int, SheetChangeRecord> Sections { get; } = [];

    public Dictionary<int, SheetChangeRecord> ColumnBlocks { get; } = [];

    public int Count => Cells.Count + Rows.Count + Sections.Count + ColumnBlocks.Count;
}
