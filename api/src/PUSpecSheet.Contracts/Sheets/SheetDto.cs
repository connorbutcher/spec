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
    IReadOnlyList<AvailableTemplateDto> AvailableTemplates);
