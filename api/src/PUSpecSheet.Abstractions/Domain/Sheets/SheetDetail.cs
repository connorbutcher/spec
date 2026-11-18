namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// A sheet to open: the published version or the draft that a spec sheet is, with the submissions that apply to it.
/// </summary>
public record SheetDetail
{
    public required SheetLocation Location { get; init; }

    public required Sheet Sheet { get; init; }

    /// <summary>
    /// The sheet's submissions, oldest first: those that published the version for a published sheet, all of them for
    /// a draft.
    /// </summary>
    public required ICollection<SheetSubmission> Submissions { get; init; } = [];
}
