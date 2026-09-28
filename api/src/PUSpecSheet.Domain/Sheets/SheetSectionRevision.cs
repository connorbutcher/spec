using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// One version of a sheet section's placement: its position among its siblings and whether it's on the
/// sheet. Versioned like rows: a <see cref="RevisionStatus.Draft"/> locks the section to its author
/// until published.
/// </summary>
public class SheetSectionRevision
{
    public int Id { get; set; }

    public int SheetSectionId { get; set; }

    public SheetSection SheetSection { get; set; } = null!;

    /// <summary>1, 2, 3… within the section.</summary>
    public int RevisionNumber { get; set; }

    public RevisionStatus Status { get; set; }

    /// <summary>Order among sibling sections as of this revision.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>True when this revision removes the section (and everything in it) from the sheet.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Who made the change; for a draft, who holds the lock.</summary>
    public int AuthorUserId { get; set; }

    public User Author { get; set; } = null!;

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

    public SheetVersion? SheetVersion { get; set; }
}
