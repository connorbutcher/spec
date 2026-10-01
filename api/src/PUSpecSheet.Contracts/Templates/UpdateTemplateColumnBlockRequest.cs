using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Renames a column block and sets how many copies a sheet table may hold: the counts must satisfy
/// min &lt;= starts with &lt;= max, and a null maximum means no limit.
/// </summary>
public sealed record UpdateTemplateColumnBlockRequest(
    [Required, MaxLength(100)] string Name,
    [Range(0, 1000)] int MinInstances,
    [Range(1, 1000)] int? MaxInstances,
    [Range(0, 1000)] int InitialInstances);
