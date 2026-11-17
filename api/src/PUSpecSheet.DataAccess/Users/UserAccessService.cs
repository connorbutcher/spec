using PUSpecSheet.Abstractions.Users;

namespace PUSpecSheet.DataAccess.Users;

public sealed class UserAccessService : IUserAccessService
{
    private UserAccess? _current;

    private readonly ICurrentUser _currentUser;
    private readonly IUserDataService _userData;

    public UserAccessService(ICurrentUser currentUser, IUserDataService userData)
    {
        _currentUser = currentUser;
        _userData = userData;
    }

    public async Task<UserAccess> GetCurrentAsync(CancellationToken cancellationToken)
    {
        if (_current is not null)
        {
            return _current;
        }

        var userId = _currentUser.UserId;
        var user = await _userData.FindWithAccessAsync(userId, cancellationToken);

        _current = user is null ? UserAccess.None(userId) : UserAccess.From(user);
        return _current;
    }

    public async Task<CurrentUser> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var access = await GetCurrentAsync(cancellationToken);
        return new CurrentUser
        {
            Id = access.UserId,
            UserName = access.UserName,
            DisplayName = access.DisplayName,
            IsAdministrator = access.IsAdministrator,
            Roles = access.Roles,
            Permissions = access.Permissions,
        };
    }
}
