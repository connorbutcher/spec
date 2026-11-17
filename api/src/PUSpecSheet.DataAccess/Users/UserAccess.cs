using PUSpecSheet.Abstractions.Users;
using PUSpecSheet.Database.Models.Users;

namespace PUSpecSheet.DataAccess.Users;

public sealed record UserAccess
{
    public required int UserId { get; init; }

    public required string UserName { get; init; }

    public required string DisplayName { get; init; }

    public required bool IsAdministrator { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }

    public required IReadOnlyList<string> Permissions { get; init; }

    public bool Has(string permission)
    {
        return IsAdministrator || Permissions.Contains(permission, StringComparer.Ordinal);
    }

    public static UserAccess None(int userId)
    {
        return new UserAccess
        {
            UserId = userId,
            UserName = string.Empty,
            DisplayName = string.Empty,
            IsAdministrator = false,
            Roles = [],
            Permissions = [],
        };
    }

    // Expects roles and their permissions loaded. Administrators get every permission whatever is listed against
    // the role; inactive users get no roles.
    public static UserAccess From(DbUser user)
    {
        var roles = user.IsActive
            ? user.Roles.Select(link => link.Role).OrderBy(role => role.Name, StringComparer.OrdinalIgnoreCase).ToList()
            : [];

        var isAdministrator = roles.Any(role => role.Id == WellKnownRoles.AdministratorId);

        var permissions = isAdministrator
            ? PermissionKeys.All
            : roles
                .SelectMany(role => role.Permissions)
                .Select(link => link.Permission.Key)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList();

        return new UserAccess
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            IsAdministrator = isAdministrator,
            Roles = roles.Select(role => role.Name).ToList(),
            Permissions = permissions,
        };
    }
}
