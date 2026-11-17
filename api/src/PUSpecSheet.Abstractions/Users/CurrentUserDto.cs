namespace PUSpecSheet.Abstractions.Users;

/// <summary>
/// The user a request runs as. <see cref="Permissions"/> is everything the user's roles allow, such as
/// <c>phases.manage</c>; an administrator has every permission listed.
/// </summary>
public sealed record CurrentUserDto
{
    public required int Id { get; init; }

    public required string UserName { get; init; }

    public required string DisplayName { get; init; }

    public required bool IsAdministrator { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }

    public required IReadOnlyList<string> Permissions { get; init; }
}
