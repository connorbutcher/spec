using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Abstractions.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Mappers.Users;
using PUSpecSheet.Database.Models.Users;

namespace PUSpecSheet.DataAccess.Users;

public sealed class UserDataService : IUserDataService
{
    private readonly PuSpecSheetDbContext _db;

    public UserDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<DbUser?> FindWithAccessAsync(int userId, CancellationToken cancellationToken)
    {
        return await _db.Users
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
        return await _db.Users
            .Where(user => user.Id == userId)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> FindActiveDisplayNameAsync(int userId, CancellationToken cancellationToken)
    {
        return await _db.Users
            .Where(user => user.Id == userId && user.IsActive)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<int, string>> GetDisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        return await _db.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
    }

    public async Task<List<string>> GetSortedDisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        return await _db.Users
            .Where(user => userIds.Contains(user.Id))
            .OrderBy(user => user.DisplayName)
            .Select(user => user.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, string>> GetAllDisplayNamesAsync(CancellationToken cancellationToken)
    {
        return await _db.Users
            .AsNoTracking()
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetIdsByUserNameAsync(IReadOnlyCollection<string> userNames, CancellationToken cancellationToken)
    {
        return await _db.Users
            .Where(user => userNames.Contains(user.UserName))
            .ToDictionaryAsync(user => user.UserName, user => user.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
    }

    public async Task<List<Approver>> ListApproversAsync(int excludedUserId, CancellationToken cancellationToken)
    {
        return await Approvers()
            .Where(user => user.Id != excludedUserId)
            .OrderBy(user => user.DisplayName)
            .Select(UserMapper.ApproverProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountApproversAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        return await Approvers()
            .Where(user => userIds.Contains(user.Id))
            .CountAsync(cancellationToken);
    }

    private IQueryable<DbUser> Approvers()
    {
        return _db.Users.Where(user => user.IsActive && user.Roles.Any(link =>
            link.RoleId == WellKnownRoles.AdministratorId
            || link.Role.Permissions.Any(permission => permission.Permission.Key == PermissionKeys.SheetsApprove)));
    }
}
