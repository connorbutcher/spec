namespace PUSpecSheet.Database;

/// <summary>
/// A copy of a <see cref="DbTemplateColumnBlock"/> on a horizontal sheet table. The block runs through
/// every row of the table: each <see cref="DbSheetRow"/> holds a <see cref="DbSheetCell"/> for each of the
/// block's template cells in that row, pointing back here. Whether the block is on the sheet, and where,
/// is versioned in <see cref="Revisions"/>.
/// </summary>
public class DbSheetColumnBlock
{
    public int Id { get; set; }

    /// <summary>Stable identifier that stays with the block across every version.</summary>
    public Guid PublicId { get; set; }

    public int SheetTableId { get; set; }

    public DbSheetTable SheetTable { get; set; } = null!;

    public int TemplateColumnBlockId { get; set; }

    public DbTemplateColumnBlock TemplateColumnBlock { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<DbSheetColumnBlockRevision> Revisions { get; set; } = [];

    public ICollection<DbSheetCell> Cells { get; set; } = [];
}
