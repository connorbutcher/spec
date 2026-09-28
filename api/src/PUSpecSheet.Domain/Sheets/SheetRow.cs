using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// A row of a sheet table, laid out from a <see cref="TemplateRow"/>. The row is the unit that is locked
/// and versioned: its cell values live in <see cref="Revisions"/>, never on the row itself.
/// </summary>
public class SheetRow
{
    public int Id { get; set; }

    public int SheetTableId { get; set; }

    public SheetTable SheetTable { get; set; } = null!;

    public int TemplateRowId { get; set; }

    public TemplateRow TemplateRow { get; set; } = null!;

    /// <summary>Order among the table's rows.</summary>
    public int DisplayOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<SheetRowRevision> Revisions { get; set; } = [];
}
