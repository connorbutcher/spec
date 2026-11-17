using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Mappers.Configuration;
using PUSpecSheet.Api.Database.Mappers.Template;
using PUSpecSheet.Api.Database.Mappers.Types;
using PUSpecSheet.Api.Database.Schema.Types;

namespace PUSpecSheet.Api.Database.Mappers.Types;

public static class SpecSheetTypeMapper
{
    public static SpecSheetType MapToDomain(this DbSpecSheetType dbSpecSheetType)
    {
        return new SpecSheetType
        {
            Id = dbSpecSheetType.Id,
            Type = dbSpecSheetType.Type,
            TableOrientation = dbSpecSheetType.TableOrientation,
            DisplayOrder = dbSpecSheetType.DisplayOrder,
            CanCopyFromMigrated = dbSpecSheetType.CanCopyFromMigrated,
            RequiresApprovalWorkflow = dbSpecSheetType.RequiresApprovalWorkflow,
            TableTemplates = [.. dbSpecSheetType.TableTemplates.MapToDomain()],
            Permissions = [.. dbSpecSheetType.Permissions.MapToDomain()],
            Properties = [.. dbSpecSheetType.Properties.MapToDomain()],
            StructureLevels = [.. dbSpecSheetType.StructureLevels.MapToDomain()],
        };
    }

    public static IEnumerable<SpecSheetType> MapToDomain(this ICollection<DbSpecSheetType> dbSpecSheetTypes)
    {
        return dbSpecSheetTypes.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetType> MapToDomain(this IQueryable<DbSpecSheetType> dbSpecSheetTypes)
    {
        return dbSpecSheetTypes
            .Select(dbSpecSheetType => dbSpecSheetType.MapToDomain());
    }
}
