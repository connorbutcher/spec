using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Mappers.Access;

public static class PermissionMapper
{
    public static Permission MapToDomain(this DbPermission dbPermission)
    {
        return new Permission
        {
            Id = dbPermission.Id,
            Description = dbPermission.Description,
            Title = dbPermission.Title,
            Key = dbPermission.Key
        };
    }

    public static IEnumerable<Permission> MapToDomain(this ICollection<DbPermission> dbPermissions)
    {
        return dbPermissions.Select(MapToDomain);
    }

    public static IQueryable<Permission> MapToDomain(this IQueryable<DbPermission> dbPermissions)
    {
        return dbPermissions.Select(x => x.MapToDomain());
    }
}
