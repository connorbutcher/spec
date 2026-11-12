using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Contracts.Users;

namespace PUSpecSheet.Application.Users;

public sealed class UserAccessService : IUserAccessService
{
    private UserAccess? current;

    private readonly IUserRepository users;
    private readonly ICurrentUser currentUser;

    public UserAccessService(IUserRepository users, ICurrentUser currentUser)
    {
        this.users = users;
        this.currentUser = currentUser;
    }

    public async Task<UserAccess> GetCurrentAsync(CancellationToken cancellationToken)
    {
        if (current is not null)
        {
            return current;
        }

        var userId = currentUser.UserId;
        var user = await users.FindWithAccessAsync(userId, cancellationToken);

        current = user is null ? UserAccess.None(userId) : UserAccess.From(user);
        return current;
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var access = await GetCurrentAsync(cancellationToken);
        return new CurrentUserDto(
            access.UserId,
            access.UserName,
            access.DisplayName,
            access.IsAdministrator,
            access.Roles,
            access.Permissions);
    }
}
