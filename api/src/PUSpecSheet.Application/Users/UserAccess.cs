using PUSpecSheet.Database.Entities.Users;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Application.Users;

/// <summary>Who a user is and what their roles allow them to do.</summary>
public sealed record UserAccess
{
    public UserAccess(
        int userId,
        string userName,
        string displayName,
        bool isAdministrator,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions)
    {
        this.UserId = userId;
        this.UserName = userName;
        this.DisplayName = displayName;
        this.IsAdministrator = isAdministrator;
        this.Roles = roles;
        this.Permissions = permissions;
    }

    public int UserId { get; init; }

    public string UserName { get; init; }

    public string DisplayName { get; init; }

    public bool IsAdministrator { get; init; }

    public IReadOnlyList<string> Roles { get; init; }

    public IReadOnlyList<string> Permissions { get; init; }

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
