namespace PUSpecSheet.Contracts.Sheets;

/// <summary>The rows of one table that a linked dropdown on the sheet can take its choices from, top to bottom.</summary>
public sealed record SheetTableLinkableRowsDto
{
    public SheetTableLinkableRowsDto(int sheetTableId, IReadOnlyList<SheetLinkableRowDto> rows)
    {
        this.SheetTableId = sheetTableId;
        this.Rows = rows;
    }

    public int SheetTableId { get; init; }

    public IReadOnlyList<SheetLinkableRowDto> Rows { get; init; }
}
