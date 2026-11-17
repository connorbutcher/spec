using PUSpecSheet.Abstractions.Domain.Configuration;

namespace PUSpecSheet.Abstractions.Domain.Template;

public record TableCellTemplate
{
    public required int Id { get; init; }

    public required int TableRowTemplateId { get; init; }

    public required int CellConfigurationId { get; init; }

    public required int TableCellTypeId { get; init; }

    public required int ColumnNumber { get; set; }

    public required int ColSpan { get; set; }

    public required int RowSpan { get; set; }

    public required string? GroupKey { get; set; }

    public virtual CellConfiguration? CellConfiguration { get; init; }

    public virtual TableCellType? TableCellType { get; init; }
}
