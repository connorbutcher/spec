using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Abstractions.Domain.Legacy.Values;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Mappers.Configuration;
using PUSpecSheet.Api.Database.Mappers.Template;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Mappers.Legacy.Values;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.SpecSheets;

public static class SpecSheetTableCellMapper
{
    public static SpecSheetTableCell MapToDomain(this DbSpecSheetTableCell dbSpecSheetTableCell)
    {
        CellValue? cellValue;
        if (dbSpecSheetTableCell.NumericValue is not null)
        {
            cellValue = dbSpecSheetTableCell.NumericValue.MapToDomain();
        }
        else if (dbSpecSheetTableCell.TextValue is not null)
        {
            cellValue = dbSpecSheetTableCell.TextValue.MapToDomain();
        }
        else if (dbSpecSheetTableCell.PairValue is not null)
        {
            cellValue = dbSpecSheetTableCell.PairValue.MapToDomain();
        }
        else if (dbSpecSheetTableCell.LinkedSpecSheetValue is not null)
        {
            cellValue = dbSpecSheetTableCell.LinkedSpecSheetValue.MapToDomain();
        }
        else
        {
            throw new Exception("SpecSheetTableCell must have either a NumericValue or TextValue.");
        }

        return new SpecSheetTableCell
        {
            Id = dbSpecSheetTableCell.Id,
            SpecSheetTableRowId = dbSpecSheetTableCell.SpecSheetTableRowId,
            CellConfigurationId = dbSpecSheetTableCell.CellConfigurationId,
            TableCellTypeId = dbSpecSheetTableCell.TableCellTypeId,
            RowNumber = dbSpecSheetTableCell.RowNumber,
            ColumnNumber = dbSpecSheetTableCell.ColumnNumber,
            ColSpan = dbSpecSheetTableCell.ColSpan,
            RowSpan = dbSpecSheetTableCell.RowSpan,
            IsDeleted = dbSpecSheetTableCell.IsDeleted,
            ReferenceId = dbSpecSheetTableCell.ReferenceId,
            CellValue = cellValue,
            CreatedByUserId = dbSpecSheetTableCell.CreatedByUserId,
            CreatedDateTime = dbSpecSheetTableCell.CreatedDateTime,
            CreatedByUser = dbSpecSheetTableCell.CreatedByUser?.MapToDomain(),
            LastModifiedByUserId = dbSpecSheetTableCell.LastModifiedByUserId,
            LastModifiedDateTime = dbSpecSheetTableCell.LastModifiedDateTime,
            LastModifiedByUser = dbSpecSheetTableCell.LastModifiedByUser?.MapToDomain(),
            CellConfiguration = dbSpecSheetTableCell.CellConfiguration?.MapToDomain(),
            TableCellType = dbSpecSheetTableCell.TableCellType?.MapToDomain(),
            ConfigurationOverride = dbSpecSheetTableCell.ConfigurationOverride,
        };
    }

    public static IQueryable<SpecSheetTableCell> MapToDomain(this IQueryable<DbSpecSheetTableCell> dbSpecSheetTableCells)
    {
        return dbSpecSheetTableCells
            .Select(db => db.MapToDomain());
    }

    public static IEnumerable<SpecSheetTableCell> MapToDomain(this ICollection<DbSpecSheetTableCell> dbSpecSheetTableCells)
    {
        return dbSpecSheetTableCells.Select(MapToDomain);
    }
}
