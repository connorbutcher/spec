using PUSpecSheet.Abstractions.Domain.Types;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// A row of a sheet's list of versions: a published version, or the draft that will be the next one, without tables.
/// </summary>
public record SheetListItem
{
    public required int SheetId { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public SpecSheetPhase? SpecSheetPhase { get; init; }

    public SpecSheetType? SpecSheetType { get; init; }

    /// <summary>
    /// The version's number; for the draft, the number it will have when it is published.
    /// </summary>
    public required int VersionNumber { get; init; }

    public required bool IsDraft { get; init; }

    /// <summary>
    /// The published version; null for the draft.
    /// </summary>
    public SheetVersion? Version { get; init; }

    /// <summary>
    /// The sheet's property values: as of the version, or for the draft those that are published now.
    /// </summary>
    public required ICollection<SheetProperty> Properties { get; init; } = [];
}
