using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.Database.Mappers.Legacy.SpecSheets;

public static class SpecSheetTableGroupMapper
{
    public static SpecSheetTableGroup MapToDomain(this DbSpecSheetTableGroup dbSpecSheetTableGroup)
    {
        return new SpecSheetTableGroup
        {
            Id = dbSpecSheetTableGroup.Id,
            SpecSheetTableId = dbSpecSheetTableGroup.SpecSheetTableId,
            TableGroupTemplateId = dbSpecSheetTableGroup.TableGroupTemplateId,
            LinkedSpecSheetTableGroupId = dbSpecSheetTableGroup.LinkedSpecSheetTableGroupId,
            Order = dbSpecSheetTableGroup.Order,
            ReferenceId = dbSpecSheetTableGroup.ReferenceId,
            IsDeleted = dbSpecSheetTableGroup.IsDeleted,
            Rows = [.. dbSpecSheetTableGroup.Rows.MapToDomain()],
            Widgets = [.. dbSpecSheetTableGroup.Widgets.MapToDomain()],
            CreatedByUserId = dbSpecSheetTableGroup.CreatedByUserId,
            CreatedDateTime = dbSpecSheetTableGroup.CreatedDateTime,
            CreatedByUser = dbSpecSheetTableGroup.CreatedByUser?.MapToDomain(),
            LastModifiedByUserId = dbSpecSheetTableGroup.LastModifiedByUserId,
            LastModifiedDateTime = dbSpecSheetTableGroup.LastModifiedDateTime,
            LastModifiedByUser = dbSpecSheetTableGroup.LastModifiedByUser?.MapToDomain(),
        };
    }

    public static IEnumerable<SpecSheetTableGroup> MapToDomain(this IEnumerable<DbSpecSheetTableGroup> dbSpecSheetTableGroups)
    {
        return dbSpecSheetTableGroups
            .Select(db => db.MapToDomain());
    }
}
