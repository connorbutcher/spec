using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Templates;

/// <summary>
/// Renames a section and, for an addable section, sets how many copies a sheet table may hold:
/// the counts must satisfy min &lt;= starts with &lt;= max, and a null maximum means no limit. The
/// counts of the header are fixed at one and are ignored.
/// </summary>
public sealed record UpdateTemplateSectionRequest
{
    public UpdateTemplateSectionRequest(
        [Required, MaxLength(100)] string name,
        [Range(0, 1000)] int minInstances,
        [Range(1, 1000)] int? maxInstances,
        [Range(0, 1000)] int initialInstances)
    {
        Name = name;
        MinInstances = minInstances;
        MaxInstances = maxInstances;
        InitialInstances = initialInstances;
    }

    public string Name { get; init; }

    public int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public int InitialInstances { get; init; }
}
