namespace PUSpecSheet.Domain.Templates;

/// <summary>Which way a table template grows on a sheet.</summary>
public enum TemplateOrientation
{
    /// <summary>
    /// Sections stack top to bottom as in a vertical table, and the table also has
    /// <see cref="TemplateColumnBlock"/>s that people add copies of left to right, each running through
    /// every row.
    /// </summary>
    Horizontal,

    /// <summary>Sections stack top to bottom; the columns are fixed.</summary>
    Vertical,
}
