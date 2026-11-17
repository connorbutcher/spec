namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>One sheet in a phase's list of sheets: what tells it apart, and where its versions and drafts stand.</summary>
public sealed record SheetSummaryDto
{
    public required int Id { get; init; }

    public required Guid PublicId { get; init; }

    public required int PhaseId { get; init; }

    public required int SheetTypeId { get; init; }

    /// <summary>The sheet's identifier values; an identifier it has no value for is left out.</summary>
    public required IReadOnlyList<SheetIdentifierValueDto> Identifiers { get; init; }

    /// <summary>Null until something has been published on the sheet.</summary>
    public int? LatestVersionNumber { get; init; }

    public DateTime? LatestPublishedAtUtc { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Unpublished changes on the sheet, by anyone: tables, sections, column blocks and rows with a draft.</summary>
    public required int DraftCount { get; init; }

    /// <summary>Submissions on the sheet waiting for approval.</summary>
    public required int PendingSubmissionCount { get; init; }
}
