using PUSpecSheet.Database.Models.Users;
using PUSpecSheet.Database.Models.Values;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Database.Models.Sheets;

/// <summary>
/// One version of a sheet row's cell values. A row has at most one <see cref="RevisionStatus.Draft"/>
/// revision at a time; that draft is the row's lock, held by <see cref="AuthorUserId"/>. Publishing
/// stamps <see cref="PublishedAtUtc"/> and ties the revision to a <see cref="DbSheetVersion"/>.
/// </summary>
public class DbSheetRowRevision : IDbSheetRevision
{
    public int Id { get; set; }

    public int SheetRowId { get; set; }

    /// <summary>
    /// The sheet the item is on, copied onto every revision so that "this sheet at a moment" is one index
    /// seek rather than a join through the item's parents. An item never moves to another sheet.
    /// </summary>
    public int SheetId { get; set; }

    /// <summary>1, 2, 3… within the row.</summary>
    public int RevisionNumber { get; set; }

    public RevisionStatus Status { get; set; }

    /// <summary>Order among the section's rows as of this revision.</summary>
    public int DisplayOrder { get; set; }

    public bool IsDeleted { get; set; }

    /// <summary>Who made the change; for a draft, who holds the row's lock.</summary>
    public int AuthorUserId { get; set; }

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

    /// <summary>
    /// The publish waiting for approval that this draft is part of. While set, the draft is frozen: nobody,
    /// its author included, can change, discard or hand it on. Kept once published, to say which approval did it.
    /// </summary>
    public int? SheetSubmissionId { get; set; }

    public virtual DbSheetRow SheetRow { get; set; } = null!;

    public virtual DbUser Author { get; set; } = null!;

    public virtual DbSheetVersion? SheetVersion { get; set; }

    // The revision's cell values, one typed table per cell kind in the "cell" schema.

    public virtual ICollection<DbTextValue> TextValues { get; set; } = [];

    public virtual ICollection<DbNumericValue> NumericValues { get; set; } = [];

    public virtual ICollection<DbDateValue> DateValues { get; set; } = [];

    public virtual ICollection<DbBooleanValue> BooleanValues { get; set; } = [];

    public virtual ICollection<DbOptionValue> OptionValues { get; set; } = [];

    /// <summary>The values of labelled text cells, one for each slot that holds something.</summary>
    public virtual ICollection<DbLabelledTextValue> LabelledTextValues { get; set; } = [];

    /// <summary>The settings chosen on the sheet for the revision's cells, kept beside their values.</summary>
    public virtual ICollection<DbCellSettingsValue> CellSettings { get; set; } = [];
}
