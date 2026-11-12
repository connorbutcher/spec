namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>Where a sheet row sits: its section and that section's table.</summary>
public sealed record SheetRowPlace(int SheetSectionId, int SheetTableId);
