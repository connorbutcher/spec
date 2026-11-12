namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A kind of column block that can be added to a horizontal table, with how many copies there are now
/// and the template's limits. A null maximum means no limit.
/// </summary>
public sealed record AddableColumnBlockDto
{
    public AddableColumnBlockDto(int templateColumnBlockId, string name, int count, int minInstances, int? maxInstances, bool canAdd)
    {
        this.TemplateColumnBlockId = templateColumnBlockId;
        this.Name = name;
        this.Count = count;
        this.MinInstances = minInstances;
        this.MaxInstances = maxInstances;
        this.CanAdd = canAdd;
    }

    public int TemplateColumnBlockId { get; init; }

    public string Name { get; init; }

    public int Count { get; init; }

    public int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public bool CanAdd { get; init; }
}
