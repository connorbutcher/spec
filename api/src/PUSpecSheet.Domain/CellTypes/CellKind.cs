namespace PUSpecSheet.Domain.CellTypes;

/// <summary>The built-in behaviour a <see cref="CellType"/> is based on.</summary>
public enum CellKind
{
    /// <summary>Fixed text shown in the template, such as a column header. Stores no value.</summary>
    Heading,

    /// <summary>A caption that groups the cells around it, such as "Bolt torques". Stores no value.</summary>
    Group,

    Text,

    Number,

    Date,

    Checkbox,

    /// <summary>A choice from the cell type's text <see cref="CellType.Options"/>.</summary>
    TextDropdown,

    /// <summary>A choice from the cell type's <see cref="CellType.Options"/>, each of which is a number.</summary>
    NumberDropdown,
}
