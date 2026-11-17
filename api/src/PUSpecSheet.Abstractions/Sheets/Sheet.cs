using PUSpecSheet.Abstractions.Templates;

namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>
/// A sheet as one viewer sees it. A live view is the latest published state with the viewer's own
/// drafts on top and other people's locks marked; a past view (by version number or date) is read-only
/// and shows only what was published then.
/// </summary>
public sealed record Sheet
{
    public required int Id { get; init; }

    public required Guid PublicId { get; init; }

    public required int PhaseId { get; init; }

    public required int SheetTypeId { get; init; }

    public required bool IsLive { get; init; }

    public int? ViewedVersionNumber { get; init; }

    public DateTime? ViewedAsOfUtc { get; init; }

    public int? LatestVersionNumber { get; init; }

    public required int MyDraftCount { get; init; }

    public required IReadOnlyList<SheetVersionSummary> Versions { get; init; }

    public required IReadOnlyList<SheetTable> Tables { get; init; }

    public required IReadOnlyList<AvailableTemplate> AvailableTemplates { get; init; }

    /// <summary>The sheet's identifier values, which tell it from the phase's other sheets of its type.</summary>
    public IReadOnlyList<SheetIdentifierValue> Identifiers { get; init; } = [];

    /// <summary>
    /// Other people's unpublished changes on the sheet, by person. Publishing everything takes these as
    /// well as the viewer's own. Empty for a past view.
    /// </summary>
    public IReadOnlyList<SheetDraftSummary> OtherDrafts { get; init; } = [];

    /// <summary>
    /// Set when the view shows a submission's changes waiting for approval over the published sheet, for its
    /// approvers to check. Such a view is read-only, like a past one.
    /// </summary>
    public int? ReviewedSubmissionId { get; init; }

    /// <summary>The choices of every column a linked dropdown on the sheet is pointed at.</summary>
    public IReadOnlyList<SheetLinkedSource> LinkedSources { get; init; } = [];

    /// <summary>
    /// The template cells the sheet's cells were built from, each once. A cell names its own by
    /// <see cref="SheetCell.TemplateCellId"/>; many cells share one, so it isn't repeated on each.
    /// </summary>
    public IReadOnlyList<TemplateCell> TemplateCells { get; init; } = [];
}
