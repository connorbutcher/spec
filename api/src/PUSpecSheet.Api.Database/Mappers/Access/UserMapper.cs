using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Mappers.Access;

public static class UserMapper
{
    public static User MapToDomain(this DbUser dbUser)
    {
        return new User
        {
            Id = dbUser.Id,
            ObjectId = dbUser.ObjectId,
            DisplayName = dbUser.DisplayName,
            EmailAddress = dbUser.EmailAddress,
            Permissions = [.. dbUser.Permissions.MapToDomain()],
            TablePermissions = [.. dbUser.TablePermissions.MapToDomain()],
        };
    }

    public static IEnumerable<User> MapToDomain(this ICollection<DbUser> dbUsers)
    {
        return dbUsers.Select(MapToDomain);
    }

    public static IQueryable<User> MapToDomain(this IQueryable<DbUser> dbUsers)
    {
        return dbUsers.Select(x => x.MapToDomain());
    }
}
