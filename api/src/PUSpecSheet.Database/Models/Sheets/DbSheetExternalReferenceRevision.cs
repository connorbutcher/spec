namespace PUSpecSheet.Database.Models.Sheets;

/// <summary>
/// What an external reference said for one version of its sheet, as pushed in by the external system. There
/// are no drafts or approvals: a revision is published as soon as it is pushed, for the version it names.
/// <para>
/// It joins the reference's timeline at that version: <see cref="PublishedAtUtc"/> is the version's publish time,
/// not when it was pushed, so the sheet "as of" that version, or any later one until the reference is next
/// pushed, shows it, read with the same range check as the other revisions. A push for a version older than
/// the reference's latest goes into the middle of the timeline, so it is superseded at once by the revision
/// after it, and the revision before it is superseded at its publish time.
/// </para>
/// <para>
/// A reference has at most one revision per version, so pushing again for the same version updates that
/// revision rather than adding one.
/// </para>
/// </summary>
public class DbSheetExternalReferenceRevision
{
    public int Id { get; set; }

    public int SheetExternalReferenceId { get; set; }

    /// <summary>The sheet the reference is on, copied onto every revision so that "this sheet at a moment" is one index seek.</summary>
    public int SheetId { get; set; }

    /// <summary>The version the external system pushed this for.</summary>
    public int SheetVersionId { get; set; }

    /// <summary>1, 2, 3… within the reference, in the order they were pushed.</summary>
    public int RevisionNumber { get; set; }

    /// <summary>True when the external system withdrew the reference as of this version.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>The publish time of <see cref="SheetVersion"/>, copied here so the timeline is a range check like the other revisions.</summary>
    public DateTime PublishedAtUtc { get; set; }

    /// <summary>
    /// The publish time of the version of the reference's next revision, which replaced this one. Null on the
    /// latest. The revisions form an unbroken [PublishedAtUtc, SupersededAtUtc) timeline.
    /// </summary>
    public DateTime? SupersededAtUtc { get; set; }

    /// <summary>When the external system pushed it, which may be well after its version was published.</summary>
    public DateTime PushedAtUtc { get; set; }

    public int ExternalId { get; set; }

    public required string SystemName { get; set; }

    public required string OwnerName { get; set; }

    public required string Url { get; set; }

    public required string Type { get; set; }

    public string? Grouping { get; set; }

    public string? SystemIconUrl { get; set; }

    public string? Description { get; set; }

    /// <summary>Concurrency token: two pushes for the same reference at once can't both win.</summary>
    public byte[] RowVersion { get; set; } = [];

    public virtual DbSheetExternalReference SheetExternalReference { get; set; } = null!;

    public virtual DbSheetVersion SheetVersion { get; set; } = null!;
}
