namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A kind of section that can be added under a table or another section, with how many copies there
/// are now and the template's limits. A null maximum means no limit.
/// </summary>
public sealed record AddableSectionDto
{
    public AddableSectionDto(int templateSectionId, string name, int count, int minInstances, int? maxInstances, bool canAdd)
    {
        this.TemplateSectionId = templateSectionId;
        this.Name = name;
        this.Count = count;
        this.MinInstances = minInstances;
        this.MaxInstances = maxInstances;
        this.CanAdd = canAdd;
    }

    public int TemplateSectionId { get; init; }

    public string Name { get; init; }

    public int Count { get; init; }

    public int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public bool CanAdd { get; init; }
}
