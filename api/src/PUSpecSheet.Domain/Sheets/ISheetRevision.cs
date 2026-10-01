namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// What every revision type (table, section and row) has in common, so the draft, lock and "as of"
/// rules can be written once. A revision is a draft until published, and the published revisions of one
/// item form an unbroken timeline of <c>[PublishedAtUtc, SupersededAtUtc)</c> ranges.
/// </summary>
public interface ISheetRevision
{
    int Id { get; }

    int RevisionNumber { get; set; }

    RevisionStatus Status { get; set; }

    int DisplayOrder { get; set; }

    bool IsDeleted { get; set; }

    int AuthorUserId { get; set; }

    DateTime CreatedAtUtc { get; set; }

    DateTime UpdatedAtUtc { get; set; }

    DateTime? PublishedAtUtc { get; set; }

    DateTime? SupersededAtUtc { get; set; }

    int? SheetVersionId { get; set; }
}
