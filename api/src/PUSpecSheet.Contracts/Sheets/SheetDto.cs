using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A sheet as one viewer sees it. A live view is the latest published state with the viewer's own
/// drafts on top and other people's locks marked; a past view (by version number or date) is read-only
/// and shows only what was published then.
/// </summary>
public sealed record SheetDto(
    int Id,
    Guid PublicId,
    int PhaseId,
    int SheetTypeId,
    bool IsLive,
    int? ViewedVersionNumber,
    DateTime? ViewedAsOfUtc,
    int? LatestVersionNumber,
    int MyDraftCount,
    IReadOnlyList<SheetVersionSummaryDto> Versions,
    IReadOnlyList<SheetTableDto> Tables,
    IReadOnlyList<AvailableTemplateDto> AvailableTemplates)
{
    /// <summary>
    /// Other people's unpublished changes on the sheet, by person. Publishing everything takes these as
    /// well as the viewer's own. Empty for a past view.
    /// </summary>
    public IReadOnlyList<SheetDraftSummaryDto> OtherDrafts { get; init; } = [];

    /// <summary>
    /// Set when the view shows a submission's changes waiting for approval over the published sheet, for its
    /// approvers to check. Such a view is read-only, like a past one.
    /// </summary>
    public int? ReviewedSubmissionId { get; init; }

    /// <summary>The choices of every column a linked dropdown on the sheet is pointed at.</summary>
    public IReadOnlyList<SheetLinkedSourceDto> LinkedSources { get; init; } = [];

    /// <summary>
    /// The template cells the sheet's cells were built from, each once. A cell names its own by
    /// <see cref="SheetCellDto.TemplateCellId"/>; many cells share one, so it isn't repeated on each.
    /// </summary>
    public IReadOnlyList<TemplateCellDto> TemplateCells { get; init; } = [];
}
