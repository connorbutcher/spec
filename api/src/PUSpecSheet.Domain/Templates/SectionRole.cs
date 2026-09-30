namespace PUSpecSheet.Domain.Templates;

/// <summary>What a <see cref="TemplateSection"/> is for when a table is filled in on a sheet.</summary>
public enum SectionRole
{
    /// <summary>
    /// The table's header: its single top-level section with the heading rows. Every table version has
    /// exactly one, it's always first, and it can't be removed or repeated.
    /// </summary>
    Header,

    /// <summary>
    /// A section people add copies of on the sheet to build up the table's data, such as a group of
    /// rows. Sections nested inside one are addable sections too, so they form a tree.
    /// </summary>
    Addable,
}
