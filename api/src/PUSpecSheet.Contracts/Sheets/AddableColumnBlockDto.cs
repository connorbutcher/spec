namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A kind of column block that can be added to a horizontal table, with how many copies there are now
/// and the template's limits. A null maximum means no limit.
/// </summary>
public sealed record AddableColumnBlockDto(
    int TemplateColumnBlockId,
    string Name,
    int Count,
    int MinInstances,
    int? MaxInstances,
    bool CanAdd);
