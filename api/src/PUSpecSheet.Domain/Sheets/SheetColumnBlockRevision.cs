using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// One version of a sheet column block's placement: its position among the table's blocks and whether
/// it's on the sheet. Versioned like sections: a <see cref="RevisionStatus.Draft"/> locks the block to
/// its author until published.
/// </summary>
public class SheetColumnBlockRevision : ISheetRevision
{
    public int Id { get; set; }

    public int SheetColumnBlockId { get; set; }

    public SheetColumnBlock SheetColumnBlock { get; set; } = null!;

    /// <summary>1, 2, 3… within the block.</summary>
    public int RevisionNumber { get; set; }

    public RevisionStatus Status { get; set; }

    /// <summary>Order among the table's column blocks, left to right, as of this revision.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>True when this revision removes the block, with its cells in every row, from the sheet.</summary>
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
