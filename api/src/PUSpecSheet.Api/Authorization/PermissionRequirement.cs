using Microsoft.AspNetCore.Authorization;

namespace PUSpecSheet.Api.Authorization;

/// <summary>The current user must hold a permission, such as <c>phases.manage</c>, through one of their roles.</summary>
public sealed record PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        this.Permission = permission;
    }

    public string Permission { get; init; }
}
