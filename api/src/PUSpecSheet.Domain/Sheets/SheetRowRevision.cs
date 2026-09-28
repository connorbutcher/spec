using PUSpecSheet.Domain.Users;
using PUSpecSheet.Domain.Values;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// One version of a sheet row's cell values. A row has at most one <see cref="RevisionStatus.Draft"/>
/// revision at a time; that draft is the row's lock, held by <see cref="AuthorUserId"/>. Publishing
/// stamps <see cref="PublishedAtUtc"/> and ties the revision to a <see cref="SheetVersion"/>.
/// </summary>
public class SheetRowRevision
{
    public int Id { get; set; }

    public int SheetRowId { get; set; }

    public SheetRow SheetRow { get; set; } = null!;

    /// <summary>1, 2, 3… within the row.</summary>
    public int RevisionNumber { get; set; }

    public RevisionStatus Status { get; set; }

    /// <summary>Order among the section's rows as of this revision.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>True when this revision removes the row from the sheet.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Who made the change; for a draft, who holds the row's lock.</summary>
    public int AuthorUserId { get; set; }

    public User Author { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>Set when published; the "as of a date" view uses this.</summary>
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

    // The revision's cell values, one typed table per cell kind in the "values" schema.

    public ICollection<TextValue> TextValues { get; set; } = [];

    public ICollection<NumericValue> NumericValues { get; set; } = [];

    public ICollection<DateValue> DateValues { get; set; } = [];

    public ICollection<BooleanValue> BooleanValues { get; set; } = [];

    public ICollection<OptionValue> OptionValues { get; set; } = [];
}
