using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.Users;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Users;

public sealed class UserAccessService(PuSpecSheetDbContext db, ICurrentUser currentUser) : IUserAccessService
{
    private UserAccess? current;

    public async Task<UserAccess> GetCurrentAsync(CancellationToken cancellationToken)
    {
        if (current is not null)
        {
            return current;
        }

        var userId = currentUser.UserId;
        var user = await db.Users
            .AsNoTracking()
            .AsSplitQuery()
            .Include(candidate => candidate.Roles)
            .ThenInclude(link => link.Role)
            .ThenInclude(role => role.Permissions)
            .ThenInclude(link => link.Permission)
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);

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
