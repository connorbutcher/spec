namespace PUSpecSheet.DataAccess.Sheets.Queries;

/// <summary>Where a sheet row sits: its section and that section's table.</summary>
public sealed record SheetRowPlace
{
    public required int SheetSectionId { get; init; }

    public required int SheetTableId { get; init; }
}
