using PUSpecSheet.Api.Database.Schema.Configuration;

namespace PUSpecSheet.Api.Database.Schema.Template;

public record DbTableCellTemplate
{
    public required int Id { get; init; }

    public required int TableRowTemplateId { get; init; }

    public required int CellConfigurationId { get; init; }

    public required int TableCellTypeId { get; init; }

    public required int ColumnNumber { get; set; }

    public required int ColSpan { get; set; }

    public required int RowSpan { get; set; }

    public required string? GroupKey { get; set; }

    public virtual DbCellConfiguration? CellConfiguration { get; init; }

    public virtual DbTableCellType? TableCellType { get; init; }
}
