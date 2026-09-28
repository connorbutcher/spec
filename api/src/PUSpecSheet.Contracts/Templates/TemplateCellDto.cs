namespace PUSpecSheet.Contracts.Templates;

public sealed record TemplateCellDto(
    int Id,
    int CellTypeId,
    int Column,
    int RowSpan,
    int ColumnSpan,
    string? Caption,
    bool IsRequired);
