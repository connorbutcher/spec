using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Database.Models.Users;

namespace PUSpecSheet.DataAccess.Users;

public interface IUserDataService
{
    Task<DbUser?> FindWithAccessAsync(int userId, CancellationToken cancellationToken);

    Task<string?> FindDisplayNameAsync(int userId, CancellationToken cancellationToken);

    Task<string?> FindActiveDisplayNameAsync(int userId, CancellationToken cancellationToken);

    Task<Dictionary<int, string>> GetDisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken);

    Task<List<string>> GetSortedDisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken);

    Task<Dictionary<int, string>> GetAllDisplayNamesAsync(CancellationToken cancellationToken);

    Task<Dictionary<string, int>> GetIdsByUserNameAsync(IReadOnlyCollection<string> userNames, CancellationToken cancellationToken);

    Task<List<Approver>> ListApproversAsync(int excludedUserId, CancellationToken cancellationToken);

    Task<int> CountApproversAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken);
}
