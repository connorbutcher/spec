using PUSpecSheet.Database.Models.Users;

namespace PUSpecSheet.Database.Models.Sheets;

// A table from another sheet shown on this one. The table is held by public id, so it can be on a sheet that doesn't
// exist yet or has since been removed.
public class DbSheetLinkedTable
{
    public int Id { get; set; }

    public Guid PublicId { get; set; }

    public int SheetId { get; set; }

    public Guid LinkedTablePublicId { get; set; }

    public int LinkedByUserId { get; set; }

    public DateTime LinkedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual DbSheet Sheet { get; set; } = null!;

    public virtual DbUser LinkedBy { get; set; } = null!;

    public virtual ICollection<DbSheetLinkedTableRevision> Revisions { get; set; } = [];
}
