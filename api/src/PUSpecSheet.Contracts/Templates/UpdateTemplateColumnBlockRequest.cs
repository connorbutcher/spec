using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Renames a column block and sets how many copies a sheet table may hold: the counts must satisfy
/// min &lt;= starts with &lt;= max, and a null maximum means no limit.
/// </summary>
public sealed record UpdateTemplateColumnBlockRequest
{
    public UpdateTemplateColumnBlockRequest(
        [Required, MaxLength(100)] string name,
        [Range(0, 1000)] int minInstances,
        [Range(1, 1000)] int? maxInstances,
        [Range(0, 1000)] int initialInstances,
        [Range(0, 50)] int stickyColumnCount)
    {
        this.Name = name;
        this.MinInstances = minInstances;
        this.MaxInstances = maxInstances;
        this.InitialInstances = initialInstances;
        this.StickyColumnCount = stickyColumnCount;
    }

    public string Name { get; init; }

    public int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public int InitialInstances { get; init; }

    public int StickyColumnCount { get; init; }
}
