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

    /// <summary>The choices of every column a linked dropdown on the sheet is pointed at.</summary>
    public IReadOnlyList<SheetLinkedSourceDto> LinkedSources { get; init; } = [];
}
