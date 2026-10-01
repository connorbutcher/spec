using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// When things on a sheet last changed, taken from the published revisions up to the moment being viewed.
/// A cell changes when its value differs from the previous revision's (or the row was added with a value);
/// a row changes when any cell does or when it is added; a section changes when a section or row directly
/// under it is added, removed or moved.
/// </summary>
public sealed class SheetChangeHistory
{
    public static SheetChangeHistory Empty { get; } = new();

    public Dictionary<int, SheetChangeDto> Cells { get; } = [];

    public Dictionary<int, SheetChangeDto> Rows { get; } = [];

    public Dictionary<int, SheetChangeDto> Sections { get; } = [];
}
