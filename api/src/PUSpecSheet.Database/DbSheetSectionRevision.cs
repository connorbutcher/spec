using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Database;

/// <summary>
/// One version of a sheet section's placement: its position among its siblings and whether it's on the
/// sheet. Versioned like rows: a <see cref="RevisionStatus.Draft"/> locks the section to its author
/// until published.
/// </summary>
public class DbSheetSectionRevision : IDbSheetRevision
{
    public int Id { get; set; }

    public int SheetSectionId { get; set; }

    public DbSheetSection SheetSection { get; set; } = null!;

    /// <summary>
    /// The sheet the item is on, copied onto every revision so that "this sheet at a moment" is one index
    /// seek rather than a join through the item's parents. An item never moves to another sheet.
    /// </summary>
    public int SheetId { get; set; }

    /// <summary>1, 2, 3… within the section.</summary>
    public int RevisionNumber { get; set; }

    public RevisionStatus Status { get; set; }

    /// <summary>Order among sibling sections as of this revision.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>True when this revision removes the section (and everything in it) from the sheet.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Who made the change; for a draft, who holds the lock.</summary>
    public int AuthorUserId { get; set; }

    public DbUser Author { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    /// <summary>
    /// When the next revision was published and replaced this one. Null on the current published
    /// revision and on drafts. Published revisions form an unbroken [PublishedAtUtc, SupersededAtUtc)
    /// timeline, so "as of" a moment is a single range check.
    /// </summary>
    public DateTime? SupersededAtUtc { get; set; }

    /// <summary>Concurrency token: a save based on a stale copy (e.g. a second browser tab) is rejected.</summary>
    public byte[] RowVersion { get; set; } = [];

    public int? SheetVersionId { get; set; }

    public DbSheetVersion? SheetVersion { get; set; }

    /// <summary>
    /// The publish waiting for approval that this draft is part of. While set, the draft is frozen: nobody,
    /// its author included, can change, discard or hand it on. Kept once published, to say which approval did it.
    /// </summary>
    public int? SheetSubmissionId { get; set; }
}
