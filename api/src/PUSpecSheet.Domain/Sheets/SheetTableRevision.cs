using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// One version of a sheet table's own details: its user-entered <see cref="Title"/>, and whether it's
/// on the sheet at all. Versioned exactly like rows: a <see cref="RevisionStatus.Draft"/> locks the
/// table's details to its author until published, and "as of a date" uses the latest published
/// revision at or before that moment.
/// </summary>
public class SheetTableRevision
{
    public int Id { get; set; }

    public int SheetTableId { get; set; }

    public SheetTable SheetTable { get; set; } = null!;

    /// <summary>1, 2, 3… within the table.</summary>
    public int RevisionNumber { get; set; }

    public RevisionStatus Status { get; set; }

    /// <summary>The table's title as the user entered it.</summary>
    public string? Title { get; set; }

    /// <summary>True when this revision removes the table from the sheet.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Who made the change; for a draft, who holds the lock.</summary>
    public int AuthorUserId { get; set; }

    public User Author { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public int? SheetVersionId { get; set; }

    public SheetVersion? SheetVersion { get; set; }
}
