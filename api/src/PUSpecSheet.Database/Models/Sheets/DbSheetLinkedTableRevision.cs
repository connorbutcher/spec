using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Database.Models.Users;

namespace PUSpecSheet.Database.Models.Sheets;

public class DbSheetLinkedTableRevision : IDbSheetRevision
{
    public int Id { get; set; }

    public int SheetLinkedTableId { get; set; }

    public int SheetId { get; set; }

    public int RevisionNumber { get; set; }

    public RevisionStatus Status { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsDeleted { get; set; }

    public int AuthorUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public DateTime? SupersededAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public int? SheetVersionId { get; set; }

    public int? SheetSubmissionId { get; set; }

    public virtual DbSheetLinkedTable SheetLinkedTable { get; set; } = null!;

    public virtual DbUser Author { get; set; } = null!;

    public virtual DbSheetVersion? SheetVersion { get; set; }
}
