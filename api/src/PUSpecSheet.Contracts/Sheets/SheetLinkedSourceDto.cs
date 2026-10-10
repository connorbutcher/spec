namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// The choices one source gives the linked dropdowns pointed at it, as the viewer sees them, with repeats
/// left out. A source is a column of a table (<see cref="TemplateCellId"/> is set: the values the column
/// holds, top to bottom) or a row of it (<see cref="SheetRowId"/> is set: the values in that row after
/// the one that names it, left to right). Only sources some cell is linked to are listed.
/// </summary>
public sealed record SheetLinkedSourceDto(int SheetTableId, int? TemplateCellId, int? SheetRowId, IReadOnlyList<string> Options);
