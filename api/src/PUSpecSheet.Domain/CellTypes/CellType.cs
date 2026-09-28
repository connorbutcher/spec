namespace PUSpecSheet.Domain.CellTypes;

/// <summary>
/// A user-defined kind of template cell, such as "Torque (Nm)" or "Pass / Fail". Each is based on a
/// <see cref="CellKind"/>; only the settings that apply to that kind are used.
/// </summary>
public class CellType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public CellKind Kind { get; set; }

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>Text only: the most characters a value can have.</summary>
    public int? MaxLength { get; set; }

    /// <summary>Number only: how many decimal places a value is shown and entered with.</summary>
    public int? DecimalPlaces { get; set; }

    /// <summary>Number only: the smallest allowed value.</summary>
    public decimal? MinValue { get; set; }

    /// <summary>Number only: the largest allowed value.</summary>
    public decimal? MaxValue { get; set; }

    /// <summary>Number only: the unit shown after the value, e.g. "Nm".</summary>
    public string? Unit { get; set; }

    /// <summary>Dropdown only: the choices, in display order.</summary>
    public ICollection<CellTypeOption> Options { get; set; } = [];
}
