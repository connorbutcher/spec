namespace PUSpecSheet.Domain.CellTypes;

/// <summary>The built-in behaviour a <see cref="CellType"/> is based on.</summary>
public enum CellKind
{
    /// <summary>Fixed text shown in the template, such as a header. Not filled in on a sheet.</summary>
    Label,

    Text,

    Number,

    Date,

    Checkbox,

    /// <summary>A choice from the cell type's <see cref="CellType.Options"/>.</summary>
    Dropdown,
}
