using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Mappers.Access;

public static class UserPermissionMapper
{
    public static UserPermission MapToDomain(this DbUserPermission dbUserPermission)
    {
        return new UserPermission
        {
            Id = dbUserPermission.Id,
            UserId = dbUserPermission.UserId,
            PermissionId = dbUserPermission.PermissionId,
            Permission = dbUserPermission.Permission?.MapToDomain()
        };
    }

    public static IEnumerable<UserPermission> MapToDomain(this ICollection<DbUserPermission> dbUserPermissions)
    {
        return dbUserPermissions.Select(MapToDomain);
    }

    public static IQueryable<UserPermission> MapToDomain(this IQueryable<DbUserPermission> dbUserPermissions)
    {
        return dbUserPermissions.Select(x => x.MapToDomain());
    }
}
