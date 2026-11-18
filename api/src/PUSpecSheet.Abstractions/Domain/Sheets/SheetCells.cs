namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// The cells of a sheet, as it stood at a version, that have the reference ids asked for.
/// </summary>
public record SheetCells
{
    public required SheetVersion Version { get; init; }

    public required int SheetId { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required IReadOnlyList<SheetTableCell> Cells { get; init; }
}
