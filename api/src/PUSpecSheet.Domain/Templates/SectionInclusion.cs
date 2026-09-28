namespace PUSpecSheet.Domain.Templates;

/// <summary>How a <see cref="TemplateSection"/> goes into a table when the table is added to a sheet.</summary>
public enum SectionInclusion
{
    /// <summary>Added with the table and can only be removed by removing the whole table, e.g. a header.</summary>
    Required,

    /// <summary>Added with the table, but can be removed on its own.</summary>
    Default,

    /// <summary>Not added with the table; it can be added later.</summary>
    Optional,
}
