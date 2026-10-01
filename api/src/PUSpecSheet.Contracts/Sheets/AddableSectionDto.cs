namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A kind of section that can be added under a table or another section, with how many copies there
/// are now and the template's limits. A null maximum means no limit.
/// </summary>
public sealed record AddableSectionDto(
    int TemplateSectionId,
    string Name,
    int Count,
    int MinInstances,
    int? MaxInstances,
    bool CanAdd);
