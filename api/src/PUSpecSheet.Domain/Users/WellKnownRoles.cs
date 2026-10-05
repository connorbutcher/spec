namespace PUSpecSheet.Domain.Users;

/// <summary>Roles that exist in every database.</summary>
public static class WellKnownRoles
{
    /// <summary>
    /// The global administrator role. It is checked as a role and implies every permission, so a new
    /// permission never has to be granted to it.
    /// </summary>
    public const int AdministratorId = 1;

    public const string AdministratorName = "Administrator";
}
