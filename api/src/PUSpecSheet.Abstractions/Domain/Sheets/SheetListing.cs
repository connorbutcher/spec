namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// The spec sheets of a phase and sheet type, for lists: every published version, and the sheet with its draft's id.
/// </summary>
public record SheetListing
{
    public required ICollection<SheetVersionSummary> Versions { get; init; } = [];

    /// <summary>
    /// The sheet without its tables; null when the phase has none of the type.
    /// </summary>
    public SheetSummary? Sheet { get; init; }
}
