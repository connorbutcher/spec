namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// The row behind one choice of a linked dropdown: the row of the source table whose value in the linked
/// column is <see cref="Option"/>, with that row's other values, left to right. When several rows hold the
/// same value, it is the first of them.
/// </summary>
public sealed record SheetLinkedRowDto(string Option, int SheetRowId, IReadOnlyList<SheetLinkedValueDto> Values);
