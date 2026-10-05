namespace PUSpecSheet.Domain.Users;

/// <summary>A named set of permissions that users are given, such as Administrator.</summary>
public class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// The permissions this role grants. The Administrator role (<see cref="WellKnownRoles.AdministratorId"/>)
    /// grants every permission without listing any.
    /// </summary>
    public ICollection<RolePermission> Permissions { get; set; } = [];

    public ICollection<UserRole> Users { get; set; } = [];
}
