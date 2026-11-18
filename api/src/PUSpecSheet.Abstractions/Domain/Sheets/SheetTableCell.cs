using PUSpecSheet.Abstractions.Domain.Configuration;
using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Abstractions.Domain.Template;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// A cell of a row. Its place, type and configuration come from its template cell, and its value and configuration
/// override from the revision of its row that is shown.
/// </summary>
public record SheetTableCell
{
    public required int Id { get; init; }

    public required int SheetTableRowId { get; init; }

    public required int TableCellTemplateId { get; init; }

    public required int TableCellTypeId { get; init; }

    public required int CellConfigurationId { get; init; }

    public required int ColumnNumber { get; init; }

    public required int ColSpan { get; init; }

    public required int RowSpan { get; init; }

    public required Guid ReferenceId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public required SheetCellValue Value { get; init; }

    public ICellProperties? ConfigurationOverride { get; init; }

    public CellConfiguration? CellConfiguration { get; init; }

    public TableCellType? TableCellType { get; init; }
}
