using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Data.Repositories.Users;

public sealed class UserRepository(PuSpecSheetDbContext db) : IUserRepository
{
    public async Task<User?> FindWithAccessAsync(int userId, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .AsSplitQuery()
            .Include(candidate => candidate.Roles)
            .ThenInclude(link => link.Role)
            .ThenInclude(role => role.Permissions)
            .ThenInclude(link => link.Permission)
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);
    }

    public async Task<string?> FindDisplayNameAsync(int userId, CancellationToken cancellationToken)
    {
        return await db.Users
            .Where(user => user.Id == userId)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> FindActiveDisplayNameAsync(int userId, CancellationToken cancellationToken)
    {
        return await db.Users
            .Where(user => user.Id == userId && user.IsActive)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<int, string>> GetDisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
    }

    public async Task<List<string>> GetSortedDisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        return await db.Users
            .Where(user => userIds.Contains(user.Id))
            .OrderBy(user => user.DisplayName)
            .Select(user => user.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, string>> GetAllDisplayNamesAsync(CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
    }

    public async Task<List<ApproverDto>> ListApproversAsync(int excludedUserId, CancellationToken cancellationToken)
    {
        return await Approvers()
            .Where(user => user.Id != excludedUserId)
            .OrderBy(user => user.DisplayName)
            .Select(user => new ApproverDto(user.Id, user.DisplayName))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountApproversAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        return await Approvers()
            .Where(user => userIds.Contains(user.Id))
            .CountAsync(cancellationToken);
    }

    /// <summary>Active users holding <see cref="PermissionKeys.SheetsApprove"/>, which every administrator does.</summary>
    private IQueryable<User> Approvers()
    {
        return db.Users.Where(user => user.IsActive && user.Roles.Any(link =>
            link.RoleId == WellKnownRoles.AdministratorId
            || link.Role.Permissions.Any(permission => permission.Permission.Key == PermissionKeys.SheetsApprove)));
    }
}
