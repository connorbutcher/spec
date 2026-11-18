
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.SpecSheets;

public static class SpecSheetTableRowMapper
{
    public static SpecSheetTableRow MapToDomain(this DbSpecSheetTableRow dbSpecSheetTableRow)
    {
        return new SpecSheetTableRow
        {
            Id = dbSpecSheetTableRow.Id,
            SpecSheetTableGroupId = dbSpecSheetTableRow.SpecSheetTableGroupId,
            TableRowTemplateId = dbSpecSheetTableRow.TableRowTemplateId,
            RowNumber = dbSpecSheetTableRow.RowNumber,
            ReferenceId = dbSpecSheetTableRow.ReferenceId,
            IsDeleted = dbSpecSheetTableRow.IsDeleted,
            Cells = [.. dbSpecSheetTableRow.Cells.MapToDomain()],
            CreatedByUserId = dbSpecSheetTableRow.CreatedByUserId,
            CreatedDateTime = dbSpecSheetTableRow.CreatedDateTime,
            CreatedByUser = dbSpecSheetTableRow.CreatedByUser?.MapToDomain(),
            LastModifiedByUserId = dbSpecSheetTableRow.LastModifiedByUserId,
            LastModifiedDateTime = dbSpecSheetTableRow.LastModifiedDateTime,
            LastModifiedByUser = dbSpecSheetTableRow.LastModifiedByUser?.MapToDomain(),
        };
    }

    public static IEnumerable<SpecSheetTableRow> MapToDomain(this ICollection<DbSpecSheetTableRow> dbSpecSheetTableRows)
    {
        return dbSpecSheetTableRows.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetTableRow> MapToDomain(this IQueryable<DbSpecSheetTableRow> dbSpecSheetTableRows)
    {
        return dbSpecSheetTableRows
            .Select(db => db.MapToDomain());
    }
}
