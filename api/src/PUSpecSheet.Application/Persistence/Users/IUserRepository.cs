using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Database.Entities.Users;

namespace PUSpecSheet.Application.Persistence.Users;

/// <summary>Users: who they are, what they may do, and the names shown for them.</summary>
public interface IUserRepository
{
    /// <summary>The user with their roles and each role's permissions, read only; null when there is no such user.</summary>
    Task<User?> FindWithAccessAsync(int userId, CancellationToken cancellationToken);

    /// <summary>The display name of a user, or null when there is no such user.</summary>
    Task<string?> FindDisplayNameAsync(int userId, CancellationToken cancellationToken);

    /// <summary>The display name of an active user, or null when there is no such active user.</summary>
    Task<string?> FindActiveDisplayNameAsync(int userId, CancellationToken cancellationToken);

    /// <summary>The display names of the users asked for, by user id.</summary>
    Task<Dictionary<int, string>> GetDisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken);

    /// <summary>The display names of the users asked for, in name order.</summary>
    Task<List<string>> GetSortedDisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken);

    /// <summary>Every user's display name, by user id.</summary>
    Task<Dictionary<int, string>> GetAllDisplayNamesAsync(CancellationToken cancellationToken);

    /// <summary>The ids of the users with any of the user names, by user name, ignoring case.</summary>
    Task<Dictionary<string, int>> GetIdsByUserNameAsync(IReadOnlyCollection<string> userNames, CancellationToken cancellationToken);

    /// <summary>Everyone but <paramref name="excludedUserId"/> who could approve, in name order.</summary>
    Task<List<ApproverDto>> ListApproversAsync(int excludedUserId, CancellationToken cancellationToken);

    /// <summary>How many of the users asked for could approve.</summary>
    Task<int> CountApproversAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken);
}
