namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A row of a sheet table that a linked dropdown elsewhere on the sheet can take its choices from, named by
/// the first value in it, such as "Pin offset (mm)". Its other values are what the dropdown would offer.
/// </summary>
public sealed record SheetLinkableRowDto
{
    public SheetLinkableRowDto(int sheetRowId, string label)
    {
        this.SheetRowId = sheetRowId;
        this.Label = label;
    }

    public int SheetRowId { get; init; }

    public string Label { get; init; }
}
