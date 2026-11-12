namespace PUSpecSheet.Domain.CellTypes;

/// <summary>The built-in behaviour a <c>CellType</c> is based on.</summary>
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

    /// <summary>A choice from the cell type's text <c>CellType.Options</c>.</summary>
    TextDropdown,

    /// <summary>A choice from the cell type's <c>CellType.Options</c>, each of which is a number.</summary>
    NumberDropdown,

    /// <summary>
    /// A choice from the values of a column in another table on the same sheet. Which table and column is
    /// chosen on the sheet, cell by cell (see <see cref="InstanceSettings.LinkedDropdownInstanceSettings"/>),
    /// and the choice is stored as its text.
    /// </summary>
    LinkedDropdown,

    /// <summary>
    /// Several short texts in one cell, each with a label beside it, such as a right-hand and a left-hand part
    /// number. The template names the values (its slots) and the sheet can change them cell by cell.
    /// </summary>
    LabelledText,
}
