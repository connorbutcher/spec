using PUSpecSheet.Abstractions.Domain.Types;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// A sheet without its tables, for lists: its latest version and the property values it has as of that version.
/// </summary>
public record SheetSummary
{
    public required int Id { get; init; }

    public required Guid ReferenceId { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public SpecSheetPhase? SpecSheetPhase { get; init; }

    public SpecSheetType? SpecSheetType { get; init; }

    /// <summary>
    /// The latest version, or null before the sheet has been published.
    /// </summary>
    public SheetVersion? LatestVersion { get; init; }

    /// <summary>
    /// The id of the unpublished SpecSheets row for this phase and sheet type, which is where its draft is listed and
    /// opened from; null when there is none.
    /// </summary>
    public required int? DraftSpecSheetId { get; init; }

    public required ICollection<SheetProperty> Properties { get; init; } = [];
}
