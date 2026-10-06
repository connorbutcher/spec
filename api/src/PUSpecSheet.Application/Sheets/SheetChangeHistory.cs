namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// When things on a sheet last changed, taken from the published revisions up to the moment being viewed.
/// A cell changes when its value differs from the previous revision's (or the row was added with a value);
/// a row changes when any cell does or when it is added; a section changes when a section or row directly
/// under it is added, removed or moved. Once built it is only read, so one instance can serve many requests.
/// </summary>
public sealed class SheetChangeHistory
{
    public static SheetChangeHistory Empty { get; } = new();

    public Dictionary<int, SheetChange> Cells { get; } = [];

    public Dictionary<int, SheetChange> Rows { get; } = [];

    public Dictionary<int, SheetChange> Sections { get; } = [];

    public Dictionary<int, SheetChange> ColumnBlocks { get; } = [];

    /// <summary>How many items have a recorded change, which is how much room the history takes when kept.</summary>
    public int Count => Cells.Count + Rows.Count + Sections.Count + ColumnBlocks.Count;
}
