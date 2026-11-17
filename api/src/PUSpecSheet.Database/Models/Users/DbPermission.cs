using System.Diagnostics.CodeAnalysis;
using PUSpecSheet.Abstractions.Users;

namespace PUSpecSheet.Database.Models.Users;

/// <summary>Something a role can allow, such as managing phases. The keys are listed in <see cref="PermissionKeys"/>.</summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Permission is the domain's own word; this isn't a code access security permission.")]
public class DbPermission
{
    public int Id { get; set; }

    /// <summary>The unique key the API and UI check, e.g. "phases.manage".</summary>
    public required string Key { get; set; }

    public required string Description { get; set; }

    public virtual ICollection<DbRolePermission> Roles { get; set; } = [];
}
