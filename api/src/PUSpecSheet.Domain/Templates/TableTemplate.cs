using PUSpecSheet.Domain.SheetTypes;

namespace PUSpecSheet.Domain.Templates;

/// <summary>
/// A type of table that can go on a sheet type. Its layout lives in <see cref="Versions"/>; the latest
/// version is the one new sheet tables use.
/// </summary>
public class TableTemplate
{
    public int Id { get; set; }

    public int SheetTypeId { get; set; }

    public SheetType SheetType { get; set; } = null!;

    /// <summary>The template name, unique within its sheet type.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Order among the templates of the same sheet type.</summary>
    public int DisplayOrder { get; set; }

    public ICollection<TableTemplateVersion> Versions { get; set; } = [];
}
