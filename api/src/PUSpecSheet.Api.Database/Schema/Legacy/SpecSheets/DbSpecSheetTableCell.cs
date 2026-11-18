using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Configuration;
using PUSpecSheet.Api.Database.Schema.Template;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;

namespace PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

public record DbSpecSheetTableCell
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

    public required int CreatedByUserId { get; init; }

    public required DateTime CreatedDateTime { get; init; }

    public virtual DbUser? CreatedByUser { get; init; }

    public int? LastModifiedByUserId { get; set; }

    public DateTime? LastModifiedDateTime { get; set; }

    public ICellProperties? ConfigurationOverride { get; set; }

    public virtual DbUser? LastModifiedByUser { get; init; }

    public virtual DbTableCellType? TableCellType { get; init; }

    public virtual DbCellConfiguration? CellConfiguration { get; init; }

    public virtual DbNumericValue? NumericValue { get; set; }

    public virtual DbTextValue? TextValue { get; set; }

    public virtual DbPairValue? PairValue { get; set; }

    public virtual DbLinkedSpecSheetValue? LinkedSpecSheetValue { get; set; }
}
