using PUSpecSheet.Application.Users.Queries;
using PUSpecSheet.Contracts.Users;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Users;

public sealed class UserAccessService : IUserAccessService
{
    private UserAccess? current;

    private readonly PuSpecSheetDbContext db;
    private readonly ICurrentUser currentUser;

    public UserAccessService(PuSpecSheetDbContext db, ICurrentUser currentUser)
    {
        this.db = db;
        this.currentUser = currentUser;
    }

    public async Task<UserAccess> GetCurrentAsync(CancellationToken cancellationToken)
    {
        if (current is not null)
        {
            return current;
        }

        var userId = currentUser.UserId;
        var user = await UserQueries.FindWithAccessAsync(db, userId, cancellationToken);

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
