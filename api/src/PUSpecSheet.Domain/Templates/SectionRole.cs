namespace PUSpecSheet.Domain.Templates;

/// <summary>What a <see cref="TemplateSection"/> is for when a table is filled in on a sheet.</summary>
public enum SectionRole
{
    /// <summary>A single block, such as a header. At most one copy of it is on a table.</summary>
    Fixed,

    /// <summary>A block people add copies of on the sheet to build up the table's data.</summary>
    Repeating,
}
