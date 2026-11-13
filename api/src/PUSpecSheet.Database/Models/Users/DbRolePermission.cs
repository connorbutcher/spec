using System.Diagnostics.CodeAnalysis;

namespace PUSpecSheet.Database.Models.Users;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Permission is the domain's own word; this isn't a code access security permission.")]
public class DbRolePermission
{
    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public virtual DbRole Role { get; set; } = null!;

    public virtual DbPermission Permission { get; set; } = null!;
}
