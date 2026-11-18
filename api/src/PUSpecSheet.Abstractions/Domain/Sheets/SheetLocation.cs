namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// Where the spec sheet with a SpecSheets id is among the sheets: the sheet, and the version it was migrated to or that
/// it is the draft of.
/// </summary>
public record SheetLocation
{
    public required int SpecSheetId { get; init; }

    public required int SheetId { get; init; }

    /// <summary>
    /// The number of the version the published spec sheet is; null for a draft.
    /// </summary>
    public required int? VersionNumber { get; init; }

    public required bool IsDraft { get; init; }
}
