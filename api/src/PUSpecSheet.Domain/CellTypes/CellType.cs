using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Domain.CellTypes;

/// <summary>
/// A user-defined kind of template cell, such as "Torque (Nm)" or "Pass / Fail". Each is based on a
/// <see cref="CellKind"/> and carries the default configuration and style its cells start with.
/// </summary>
public class CellType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public CellKind Kind { get; set; }

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>The default settings for cells of this type. Always matches <see cref="Kind"/>.</summary>
    public CellConfiguration Configuration { get; set; } = new TextCellConfiguration();

    /// <summary>The default look of cells of this type.</summary>
    public CellStyle Style { get; set; } = new();

    /// <summary>Dropdown kinds only: the choices, in display order.</summary>
    public ICollection<CellTypeOption> Options { get; set; } = [];
}
