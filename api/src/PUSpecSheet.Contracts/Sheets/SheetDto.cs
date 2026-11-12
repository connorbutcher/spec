using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A sheet as one viewer sees it. A live view is the latest published state with the viewer's own
/// drafts on top and other people's locks marked; a past view (by version number or date) is read-only
/// and shows only what was published then.
/// </summary>
public sealed record SheetDto
{
    public SheetDto(
        int id,
        Guid publicId,
        int phaseId,
        int sheetTypeId,
        bool isLive,
        int? viewedVersionNumber,
        DateTime? viewedAsOfUtc,
        int? latestVersionNumber,
        int myDraftCount,
        IReadOnlyList<SheetVersionSummaryDto> versions,
        IReadOnlyList<SheetTableDto> tables,
        IReadOnlyList<AvailableTemplateDto> availableTemplates)
    {
        this.Id = id;
        this.PublicId = publicId;
        this.PhaseId = phaseId;
        this.SheetTypeId = sheetTypeId;
        this.IsLive = isLive;
        this.ViewedVersionNumber = viewedVersionNumber;
        this.ViewedAsOfUtc = viewedAsOfUtc;
        this.LatestVersionNumber = latestVersionNumber;
        this.MyDraftCount = myDraftCount;
        this.Versions = versions;
        this.Tables = tables;
        this.AvailableTemplates = availableTemplates;
    }

    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int PhaseId { get; init; }

    public int SheetTypeId { get; init; }

    public bool IsLive { get; init; }

    public int? ViewedVersionNumber { get; init; }

    public DateTime? ViewedAsOfUtc { get; init; }

    public int? LatestVersionNumber { get; init; }

    public int MyDraftCount { get; init; }

    public IReadOnlyList<SheetVersionSummaryDto> Versions { get; init; }

    public IReadOnlyList<SheetTableDto> Tables { get; init; }

    public IReadOnlyList<AvailableTemplateDto> AvailableTemplates { get; init; }

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
