using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Mappers.Access;

public static class SpecSheetTablePermissionMapper
{
    public static SpecSheetTablePermission MapToDomain(this DbSpecSheetTablePermission dbSpecSheetTablePermission)
    {
        return new SpecSheetTablePermission
        {
            Id = dbSpecSheetTablePermission.Id,
            UserId = dbSpecSheetTablePermission.UserId,
            TableReferenceId = dbSpecSheetTablePermission.TableReferenceId,
            IsAdmin = dbSpecSheetTablePermission.IsAdmin,
        };
    }

    public static IEnumerable<SpecSheetTablePermission> MapToDomain(this ICollection<DbSpecSheetTablePermission> dbSpecSheetTablePermissions)
    {
        return dbSpecSheetTablePermissions.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetTablePermission> MapToDomain(this IQueryable<DbSpecSheetTablePermission> dbSpecSheetTablePermissions)
    {
        return dbSpecSheetTablePermissions.Select(p => p.MapToDomain());
    }
}
