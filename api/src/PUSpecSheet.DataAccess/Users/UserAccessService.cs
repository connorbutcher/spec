using PUSpecSheet.Abstractions.Users;
using PUSpecSheet.DataAccess.Users.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Users;

public sealed class UserAccessService : IUserAccessService
{
    private UserAccess? _current;

    private readonly PuSpecSheetDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UserAccessService(PuSpecSheetDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<UserAccess> GetCurrentAsync(CancellationToken cancellationToken)
    {
        if (_current is not null)
        {
            return _current;
        }

        var userId = _currentUser.UserId;
        var user = await UserQueries.FindWithAccessAsync(_db, userId, cancellationToken);

        _current = user is null ? UserAccess.None(userId) : UserAccess.From(user);
        return _current;
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
