using System.Diagnostics.CodeAnalysis;

namespace PUSpecSheet.Domain.Users;

/// <summary>Grants a permission to a role.</summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Permission is the domain's own word; this isn't a code access security permission.")]
public class RolePermission
{
    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public int PermissionId { get; set; }

    public Permission Permission { get; set; } = null!;
}
