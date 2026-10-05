namespace PUSpecSheet.Contracts.Users;

/// <summary>
/// The user a request runs as. <see cref="Permissions"/> is everything the user's roles allow, such as
/// <c>phases.manage</c>; an administrator has every permission listed.
/// </summary>
public sealed record CurrentUserDto(
    int Id,
    string UserName,
    string DisplayName,
    bool IsAdministrator,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
