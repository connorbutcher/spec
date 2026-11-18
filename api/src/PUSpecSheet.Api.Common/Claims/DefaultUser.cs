namespace PUSpecSheet.Api.Common.Claims;

/// <summary>
/// The one user every request acts as while auth is disabled for local running.
/// </summary>
/// <remarks>
/// database/seed.sql creates this user with every admin permission (seed.ps1 passes these values by default), so keep
/// the two in step.
/// </remarks>
public static class DefaultUser
{
    /// <summary>
    /// The default user's object id, standing in for an Entra ID oid claim.
    /// </summary>
    public static readonly Guid ObjectId = new("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// The default user's display name.
    /// </summary>
    public const string Name = "Local Admin";

    /// <summary>
    /// The default user's email address.
    /// </summary>
    public const string Email = "local.admin@localhost";
}
