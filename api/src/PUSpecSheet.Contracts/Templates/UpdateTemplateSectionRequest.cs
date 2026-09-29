using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Renames a section and sets its role. For a fixed section the maximum is always 1; for a repeating
/// one a null maximum means no limit. Counts must satisfy min &lt;= initial &lt;= max.
/// </summary>
public sealed record UpdateTemplateSectionRequest(
    [Required, MaxLength(100)] string Name,
    SectionRole Role,
    [Range(0, 1000)] int MinInstances,
    [Range(1, 1000)] int? MaxInstances,
    [Range(0, 1000)] int InitialInstances);
