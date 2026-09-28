using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// One publish of a sheet. Version N shows the sheet as it stood at <see cref="PublishedAtUtc"/>: every
/// table, section and row at its latest revision published at or before that moment.
/// </summary>
public class SheetVersion
{
    public int Id { get; set; }

    public int SheetId { get; set; }

    public Sheet Sheet { get; set; } = null!;

    /// <summary>1, 2, 3… within the sheet.</summary>
    public int VersionNumber { get; set; }

    public DateTime PublishedAtUtc { get; set; }

    public int PublishedByUserId { get; set; }

    public User PublishedBy { get; set; } = null!;

    public string? Note { get; set; }

    /// <summary>The table revisions (titles, tables added, moved or removed) published in this version.</summary>
    public ICollection<SheetTableRevision> TableRevisions { get; set; } = [];

    /// <summary>The section revisions (sections added, moved or removed) published in this version.</summary>
    public ICollection<SheetSectionRevision> SectionRevisions { get; set; } = [];

    /// <summary>The row revisions published in this version.</summary>
    public ICollection<SheetRowRevision> RowRevisions { get; set; } = [];
}
