using PUSpecSheet.Contracts.Users;

namespace PUSpecSheet.Application.Users;

/// <summary>What the user the current request runs as is allowed to do.</summary>
public interface IUserAccessService
{
    /// <summary>The current user's access, read from the database once per request.</summary>
    Task<UserAccess> GetCurrentAsync(CancellationToken cancellationToken);

    /// <summary>The current user with their roles and effective permissions, for the UI.</summary>
    Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken);
}
