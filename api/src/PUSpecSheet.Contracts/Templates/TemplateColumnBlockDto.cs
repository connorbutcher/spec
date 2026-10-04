namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// A column block of a horizontal table: people add copies of it side by side on a sheet, and each copy
/// runs through every row. Its cells are the row cells whose <see cref="TemplateCellDto.ColumnBlockId"/>
/// is this block. A null maximum means no limit.
/// </summary>
public sealed record TemplateColumnBlockDto(
    int Id,
    string Name,
    int DisplayOrder,
    int MinInstances,
    int? MaxInstances,
    int InitialInstances,
    int StickyColumnCount);
