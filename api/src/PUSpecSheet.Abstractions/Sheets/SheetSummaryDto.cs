namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>One sheet in a phase's list of sheets: what tells it apart, and where its versions and drafts stand.</summary>
public sealed record SheetSummaryDto
{
    public SheetSummaryDto(
        int id,
        Guid publicId,
        int phaseId,
        int sheetTypeId,
        IReadOnlyList<SheetIdentifierValueDto> identifiers,
        int? latestVersionNumber,
        DateTime? latestPublishedAtUtc,
        DateTime createdAtUtc,
        int draftCount,
        int pendingSubmissionCount)
    {
        Id = id;
        PublicId = publicId;
        PhaseId = phaseId;
        SheetTypeId = sheetTypeId;
        Identifiers = identifiers;
        LatestVersionNumber = latestVersionNumber;
        LatestPublishedAtUtc = latestPublishedAtUtc;
        CreatedAtUtc = createdAtUtc;
        DraftCount = draftCount;
        PendingSubmissionCount = pendingSubmissionCount;
    }

    public int Id { get; init; }

    public Guid PublicId { get; init; }

    public int PhaseId { get; init; }

    public int SheetTypeId { get; init; }

    /// <summary>The sheet's identifier values; an identifier it has no value for is left out.</summary>
    public IReadOnlyList<SheetIdentifierValueDto> Identifiers { get; init; }

    /// <summary>Null until something has been published on the sheet.</summary>
    public int? LatestVersionNumber { get; init; }

    public DateTime? LatestPublishedAtUtc { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Unpublished changes on the sheet, by anyone: tables, sections, column blocks and rows with a draft.</summary>
    public int DraftCount { get; init; }

    /// <summary>Submissions on the sheet waiting for approval.</summary>
    public int PendingSubmissionCount { get; init; }
}
