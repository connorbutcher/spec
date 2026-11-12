namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>Where a sheet row sits: its section and that section's table.</summary>
public sealed record SheetRowPlace
{
    public SheetRowPlace(int sheetSectionId, int sheetTableId)
    {
        this.SheetSectionId = sheetSectionId;
        this.SheetTableId = sheetTableId;
    }

    public int SheetSectionId { get; init; }

    public int SheetTableId { get; init; }
}
