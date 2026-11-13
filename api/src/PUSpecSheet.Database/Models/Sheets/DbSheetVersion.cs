using PUSpecSheet.Database.Models.Users;

namespace PUSpecSheet.Database.Models.Sheets;

/// <summary>
/// One publish of a sheet. Version N shows the sheet as it stood at <see cref="PublishedAtUtc"/>: every
/// table, section and row at its latest revision published at or before that moment.
/// </summary>
public class DbSheetVersion
{
    public int Id { get; set; }

    public int SheetId { get; set; }

    /// <summary>1, 2, 3… within the sheet.</summary>
    public int VersionNumber { get; set; }

    public DateTime PublishedAtUtc { get; set; }

    public int PublishedByUserId { get; set; }

    public string? Note { get; set; }

    public virtual DbSheet Sheet { get; set; } = null!;

    public virtual DbUser PublishedBy { get; set; } = null!;

    /// <summary>The table revisions (titles, tables added, moved or removed) published in this version.</summary>
    public virtual ICollection<DbSheetTableRevision> TableRevisions { get; set; } = [];

    /// <summary>The section revisions (sections added, moved or removed) published in this version.</summary>
    public virtual ICollection<DbSheetSectionRevision> SectionRevisions { get; set; } = [];

    /// <summary>The column block revisions (blocks added, moved or removed) published in this version.</summary>
    public virtual ICollection<DbSheetColumnBlockRevision> ColumnBlockRevisions { get; set; } = [];

    public virtual ICollection<DbSheetRowRevision> RowRevisions { get; set; } = [];
}
