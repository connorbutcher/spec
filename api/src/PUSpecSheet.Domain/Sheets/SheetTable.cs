using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>A table on a sheet, laid out from a <see cref="TableTemplate"/> of the sheet's type.</summary>
public class SheetTable
{
    public int Id { get; set; }

    public int SheetId { get; set; }

    public Sheet Sheet { get; set; } = null!;

    public int TableTemplateId { get; set; }

    public TableTemplate TableTemplate { get; set; } = null!;

    /// <summary>Order among the sheet's tables.</summary>
    public int DisplayOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<SheetRow> Rows { get; set; } = [];
}
