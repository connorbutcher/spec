namespace PUSpecSheet.Abstractions.Templates;

/// <summary>
/// A column block of a horizontal table: people add copies of it side by side on a sheet, and each copy
/// runs through every row. Its cells are the row cells whose <see cref="TemplateCellDto.ColumnBlockId"/>
/// is this block. A null maximum means no limit.
/// </summary>
public sealed record TemplateColumnBlockDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required int DisplayOrder { get; init; }

    public required int MinInstances { get; init; }

    public int? MaxInstances { get; init; }

    public required int InitialInstances { get; init; }

    public required int StickyColumnCount { get; init; }
}
