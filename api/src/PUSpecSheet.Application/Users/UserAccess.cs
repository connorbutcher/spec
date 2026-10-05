using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Application.Users;

/// <summary>Who a user is and what their roles allow them to do.</summary>
public sealed record UserAccess(
    int UserId,
    string UserName,
    string DisplayName,
    bool IsAdministrator,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions)
{
    /// <summary>Whether the user may do what <paramref name="permission"/> names. An administrator may do anything.</summary>
    public bool Has(string permission)
    {
        return IsAdministrator || Permissions.Contains(permission, StringComparer.Ordinal);
    }

    /// <summary>The access of a user that doesn't exist: nothing.</summary>
    public static UserAccess None(int userId)
    {
        return new UserAccess(userId, string.Empty, string.Empty, false, [], []);
    }

    /// <summary>
    /// Works out a user's access from their roles. Expects the user's roles, and each role's permissions,
    /// to be loaded. The Administrator role is recognised as a role and gives every permission, whatever
    /// is listed against it; an inactive user has no roles to speak of.
    /// </summary>
    public static UserAccess From(User user)
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

        return new UserAccess(
            user.Id,
            user.UserName,
            user.DisplayName,
            isAdministrator,
            roles.Select(role => role.Name).ToList(),
            permissions);
    }
}
