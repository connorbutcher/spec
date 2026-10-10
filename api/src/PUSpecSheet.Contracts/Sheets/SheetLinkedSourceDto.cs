namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// The choices a column gives the linked dropdowns pointed at it: the values the column holds, as the
/// viewer sees them, top to bottom with repeats left out. Only columns some cell is linked to are listed.
/// </summary>
public sealed record SheetLinkedSourceDto(int SheetTableId, int TemplateCellId, IReadOnlyList<string> Options)
{
    /// <summary>
    /// The row behind each choice, when a cell linked to this column also shows its choice's row. Empty
    /// when none does.
    /// </summary>
    public IReadOnlyList<SheetLinkedRowDto> Rows { get; init; } = [];
}
