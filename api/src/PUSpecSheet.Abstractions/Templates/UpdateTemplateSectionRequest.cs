using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Templates;

/// <summary>
/// Renames a section and, for an addable section, sets how many copies a sheet table may hold:
/// the counts must satisfy min &lt;= starts with &lt;= max, and a null maximum means no limit. The
/// counts of the header are fixed at one and are ignored.
/// </summary>
public sealed record UpdateTemplateSectionRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [Range(0, 1000)]
    public required int MinInstances { get; init; }

    [Range(1, 1000)]
    public int? MaxInstances { get; init; }

    [Range(0, 1000)]
    public required int InitialInstances { get; init; }
}
