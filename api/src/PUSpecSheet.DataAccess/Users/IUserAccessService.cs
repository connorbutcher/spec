using PUSpecSheet.Abstractions.Users;

namespace PUSpecSheet.DataAccess.Users;

public interface IUserAccessService
{
    Task<UserAccess> GetCurrentAsync(CancellationToken cancellationToken);

    Task<CurrentUser> GetCurrentUserAsync(CancellationToken cancellationToken);
}
