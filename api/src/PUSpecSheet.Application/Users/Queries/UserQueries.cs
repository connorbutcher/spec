using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Database;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Application.Users.Queries;

/// <summary>Users: who they are, what they may do, and the names shown for them.</summary>
public static class UserQueries
{
    /// <summary>The user with their roles and each role's permissions, read only; null when there is no such user.</summary>
    public static async Task<DbUser?> FindWithAccessAsync(PuSpecSheetDbContext db, int userId, CancellationToken cancellationToken)
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

    /// <summary>The display name of a user, or null when there is no such user.</summary>
    public static async Task<string?> FindDisplayNameAsync(PuSpecSheetDbContext db, int userId, CancellationToken cancellationToken)
    {
        return await db.Users
            .Where(user => user.Id == userId)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>The display name of an active user, or null when there is no such active user.</summary>
    public static async Task<string?> FindActiveDisplayNameAsync(PuSpecSheetDbContext db, int userId, CancellationToken cancellationToken)
    {
        return await db.Users
            .Where(user => user.Id == userId && user.IsActive)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>The display names of the users asked for, by user id.</summary>
    public static async Task<Dictionary<int, string>> GetDisplayNamesAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
    }

    /// <summary>The display names of the users asked for, in name order.</summary>
    public static async Task<List<string>> GetSortedDisplayNamesAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        return await db.Users
            .Where(user => userIds.Contains(user.Id))
            .OrderBy(user => user.DisplayName)
            .Select(user => user.DisplayName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Every user's display name, by user id.</summary>
    public static async Task<Dictionary<int, string>> GetAllDisplayNamesAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
    }

    /// <summary>The ids of the users with any of the user names, by user name, ignoring case.</summary>
    public static async Task<Dictionary<string, int>> GetIdsByUserNameAsync(PuSpecSheetDbContext db, IReadOnlyCollection<string> userNames, CancellationToken cancellationToken)
    {
        return await db.Users
            .Where(user => userNames.Contains(user.UserName))
            .ToDictionaryAsync(user => user.UserName, user => user.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
    }

    /// <summary>Everyone but <paramref name="excludedUserId"/> who could approve, in name order.</summary>
    public static async Task<List<ApproverDto>> ListApproversAsync(PuSpecSheetDbContext db, int excludedUserId, CancellationToken cancellationToken)
    {
        return await Approvers(db)
            .Where(user => user.Id != excludedUserId)
            .OrderBy(user => user.DisplayName)
            .Select(user => new ApproverDto(user.Id, user.DisplayName))
            .ToListAsync(cancellationToken);
    }

    /// <summary>How many of the users asked for could approve.</summary>
    public static async Task<int> CountApproversAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        return await Approvers(db)
            .Where(user => userIds.Contains(user.Id))
            .CountAsync(cancellationToken);
    }

    /// <summary>Active users holding <see cref="PermissionKeys.SheetsApprove"/>, which every administrator does.</summary>
    private static IQueryable<DbUser> Approvers(PuSpecSheetDbContext db)
    {
        return db.Users.Where(user => user.IsActive && user.Roles.Any(link =>
            link.RoleId == WellKnownRoles.AdministratorId
            || link.Role.Permissions.Any(permission => permission.Permission.Key == PermissionKeys.SheetsApprove)));
    }
}
