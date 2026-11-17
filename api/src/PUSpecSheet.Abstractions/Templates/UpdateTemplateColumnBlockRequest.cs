using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Templates;

/// <summary>
/// Renames a column block and sets how many copies a sheet table may hold: the counts must satisfy
/// min &lt;= starts with &lt;= max, and a null maximum means no limit.
/// </summary>
public sealed record UpdateTemplateColumnBlockRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [Range(0, 1000)]
    public required int MinInstances { get; init; }

    [Range(1, 1000)]
    public int? MaxInstances { get; init; }

    [Range(0, 1000)]
    public required int InitialInstances { get; init; }

    [Range(0, 50)]
    public required int StickyColumnCount { get; init; }
}
