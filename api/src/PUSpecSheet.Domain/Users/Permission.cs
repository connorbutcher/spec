using System.Diagnostics.CodeAnalysis;

namespace PUSpecSheet.Domain.Users;

/// <summary>Something a role can allow, such as managing phases. The keys are listed in <see cref="PermissionKeys"/>.</summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Permission is the domain's own word; this isn't a code access security permission.")]
public class Permission
{
    public int Id { get; set; }

    /// <summary>The unique key the API and UI check, e.g. "phases.manage".</summary>
    public string Key { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> Roles { get; set; } = [];
}
