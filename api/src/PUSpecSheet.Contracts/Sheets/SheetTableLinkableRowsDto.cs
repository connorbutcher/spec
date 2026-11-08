namespace PUSpecSheet.Contracts.Sheets;

/// <summary>The rows of one table that a linked dropdown on the sheet can take its choices from, top to bottom.</summary>
public sealed record SheetTableLinkableRowsDto(int SheetTableId, IReadOnlyList<SheetLinkableRowDto> Rows);
