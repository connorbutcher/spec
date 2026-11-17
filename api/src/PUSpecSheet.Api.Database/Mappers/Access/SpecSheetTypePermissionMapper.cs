using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Mappers.Access;

public static class SpecSheetTypePermissionMapper
{
    public static SpecSheetTypePermission MapToDomain(this DbSpecSheetTypePermission dbSpecSheetTypePermission)
    {
        return new SpecSheetTypePermission
        {
            Id = dbSpecSheetTypePermission.Id,
            PermissionId = dbSpecSheetTypePermission.PermissionId,
            SpecSheetTypeId = dbSpecSheetTypePermission.SpecSheetTypeId,
            Permission = dbSpecSheetTypePermission.Permission?.MapToDomain(),
        };
    }

    public static IEnumerable<SpecSheetTypePermission> MapToDomain(this ICollection<DbSpecSheetTypePermission> dbSpecSheetTypePermissions)
    {
        return dbSpecSheetTypePermissions.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetTypePermission> MapToDomain(this IQueryable<DbSpecSheetTypePermission> dbSpecSheetTypePermissions)
    {
        return dbSpecSheetTypePermissions.Select(x => x.MapToDomain());
    }
}
