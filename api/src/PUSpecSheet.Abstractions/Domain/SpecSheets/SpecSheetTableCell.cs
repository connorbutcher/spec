using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Configuration;
using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Abstractions.Domain.Values;

namespace PUSpecSheet.Abstractions.Domain.SpecSheets;

public record SpecSheetTableCell
{
    public required int Id { get; init; }

    public required int SpecSheetTableRowId { get; init; }

    public required int TableCellTypeId { get; init; }

    public required int CellConfigurationId { get; init; }

    public required int RowNumber { get; set; }

    public required int ColumnNumber { get; set; }

    public required int ColSpan { get; set; }

    public required int RowSpan { get; set; }

    public required Guid ReferenceId { get; init; }

    public required bool IsDeleted { get; set; }

    public required CellValue? CellValue { get; set; }

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual User? CreatedByUser { get; init; }

    public int? LastModifiedByUserId { get; set; }

    public DateTime? LastModifiedDateTime { get; set; }

    public virtual User? LastModifiedByUser { get; init; }

    public ICellProperties? ConfigurationOverride { get; init; }

    public virtual CellConfiguration? CellConfiguration { get; init; }

    public virtual TableCellType? TableCellType { get; init; }
}
