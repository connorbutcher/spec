using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Database.Models.Users;

/// <summary>A named set of permissions that users are given, such as Administrator.</summary>
public class DbRole
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// The permissions this role grants. The Administrator role (<see cref="WellKnownRoles.AdministratorId"/>)
    /// grants every permission without listing any.
    /// </summary>
    public virtual ICollection<DbRolePermission> Permissions { get; set; } = [];

    public virtual ICollection<DbUserRole> Users { get; set; } = [];
}
