namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>The rows of one table that a linked dropdown on the sheet can take its choices from, top to bottom.</summary>
public sealed record SheetTableLinkableRows
{
    public required int SheetTableId { get; init; }

    public required IReadOnlyList<SheetLinkableRow> Rows { get; init; }
}
