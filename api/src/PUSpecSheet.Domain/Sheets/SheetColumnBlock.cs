using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// A copy of a <see cref="TemplateColumnBlock"/> on a horizontal sheet table. The block runs through
/// every row of the table: each <see cref="SheetRow"/> holds a <see cref="SheetCell"/> for each of the
/// block's template cells in that row, pointing back here. Whether the block is on the sheet, and where,
/// is versioned in <see cref="Revisions"/>.
/// </summary>
public class SheetColumnBlock
{
    public int Id { get; set; }

    /// <summary>Stable identifier that stays with the block across every version.</summary>
    public Guid PublicId { get; set; }

    public int SheetTableId { get; set; }

    public SheetTable SheetTable { get; set; } = null!;

    public int TemplateColumnBlockId { get; set; }

    public TemplateColumnBlock TemplateColumnBlock { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<SheetColumnBlockRevision> Revisions { get; set; } = [];

    /// <summary>The block's cells in every row of the table.</summary>
    public ICollection<SheetCell> Cells { get; set; } = [];
}
