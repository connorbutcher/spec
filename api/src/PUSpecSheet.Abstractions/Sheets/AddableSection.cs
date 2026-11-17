namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>
/// A kind of section that can be added under a table or another section, with how many copies there
/// are now and the template's limits. A null maximum means no limit.
/// </summary>
public sealed record AddableSection
{
    public required int TemplateSectionId { get; init; }

    public required string Name { get; init; }

    public required int Count { get; init; }

    public required int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public required bool CanAdd { get; init; }
}
