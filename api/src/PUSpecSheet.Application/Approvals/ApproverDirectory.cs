using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Application.Approvals;

/// <summary>
/// Who can approve submissions: active users holding <see cref="PermissionKeys.SheetsApprove"/>, which every
/// administrator does. Nobody approves their own submission, so the submitter is never offered.
/// </summary>
public sealed class ApproverDirectory(PuSpecSheetDbContext db)
{
    /// <summary>Everyone but <paramref name="submitterUserId"/> who could approve, by name.</summary>
    public async Task<IReadOnlyList<ApproverDto>> ListAsync(int submitterUserId, CancellationToken cancellationToken)
    {
        return await Approvers()
            .Where(user => user.Id != submitterUserId)
            .OrderBy(user => user.DisplayName)
            .Select(user => new ApproverDto(user.Id, user.DisplayName))
            .ToListAsync(cancellationToken);
    }

    /// <summary>Refuses a choice of approvers that is empty, names the submitter, or names someone who can't approve.</summary>
    /// <exception cref="InvalidRequestException">The choice can't be used.</exception>
    public async Task<IReadOnlyList<int>> EnsureEligibleAsync(
        IReadOnlyCollection<int> userIds,
        int submitterUserId,
        CancellationToken cancellationToken)
    {
        var chosen = userIds.Distinct().ToList();
        if (chosen.Count == 0)
        {
            throw new InvalidRequestException("Choose at least one person to approve the changes.");
        }

        if (chosen.Contains(submitterUserId))
        {
            throw new InvalidRequestException("You can't approve your own changes. Choose someone else.");
        }

        var eligible = await Approvers()
            .Where(user => chosen.Contains(user.Id))
            .CountAsync(cancellationToken);
        if (eligible != chosen.Count)
        {
            throw new InvalidRequestException("Some of the people chosen can't approve changes. Choose again.");
        }

        return chosen;
    }

    private IQueryable<User> Approvers()
    {
        return db.Users.Where(user => user.IsActive && user.Roles.Any(link =>
            link.RoleId == WellKnownRoles.AdministratorId
            || link.Role.Permissions.Any(permission => permission.Permission.Key == PermissionKeys.SheetsApprove)));
    }
}
