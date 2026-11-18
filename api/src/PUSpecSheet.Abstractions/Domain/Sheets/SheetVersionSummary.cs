using PUSpecSheet.Abstractions.Domain.Types;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// A published version of a sheet without its tables, for lists: the sheet's property values as of the version.
/// </summary>
public record SheetVersionSummary
{
    public required int SheetId { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public SpecSheetPhase? SpecSheetPhase { get; init; }

    public SpecSheetType? SpecSheetType { get; init; }

    public required SheetVersion Version { get; init; }

    public required ICollection<SheetProperty> Properties { get; init; } = [];
}
