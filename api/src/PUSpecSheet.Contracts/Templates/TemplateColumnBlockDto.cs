namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// A column block of a horizontal table: people add copies of it side by side on a sheet, and each copy
/// runs through every row. Its cells are the row cells whose <see cref="TemplateCellDto.ColumnBlockId"/>
/// is this block. A null maximum means no limit.
/// </summary>
public sealed record TemplateColumnBlockDto
{
    public TemplateColumnBlockDto(
        int id,
        string name,
        int displayOrder,
        int minInstances,
        int? maxInstances,
        int initialInstances,
        int stickyColumnCount)
    {
        this.Id = id;
        this.Name = name;
        this.DisplayOrder = displayOrder;
        this.MinInstances = minInstances;
        this.MaxInstances = maxInstances;
        this.InitialInstances = initialInstances;
        this.StickyColumnCount = stickyColumnCount;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public int DisplayOrder { get; init; }

    public int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public int InitialInstances { get; init; }

    public int StickyColumnCount { get; init; }
}
