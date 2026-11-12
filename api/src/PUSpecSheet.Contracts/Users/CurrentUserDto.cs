namespace PUSpecSheet.Contracts.Users;

/// <summary>
/// The user a request runs as. <see cref="Permissions"/> is everything the user's roles allow, such as
/// <c>phases.manage</c>; an administrator has every permission listed.
/// </summary>
public sealed record CurrentUserDto
{
    public CurrentUserDto(
        int id,
        string userName,
        string displayName,
        bool isAdministrator,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions)
    {
        this.Id = id;
        this.UserName = userName;
        this.DisplayName = displayName;
        this.IsAdministrator = isAdministrator;
        this.Roles = roles;
        this.Permissions = permissions;
    }

    public int Id { get; init; }

    public string UserName { get; init; }

    public string DisplayName { get; init; }

    public bool IsAdministrator { get; init; }

    public IReadOnlyList<string> Roles { get; init; }

    public IReadOnlyList<string> Permissions { get; init; }
}
