using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Users;

namespace PUSpecSheet.DataAccess.Approvals;

// Nobody approves their own submission, so the submitter is never offered.
public sealed class ApproverDirectory
{
    private readonly IUserDataService _userData;

    public ApproverDirectory(IUserDataService userData)
    {
        _userData = userData;
    }

    public async Task<IReadOnlyList<Approver>> ListAsync(int submitterUserId, CancellationToken cancellationToken)
    {
        return await _userData.ListApproversAsync(submitterUserId, cancellationToken);
    }

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

        var eligible = await _userData.CountApproversAsync(chosen, cancellationToken);
        if (eligible != chosen.Count)
        {
            throw new InvalidRequestException("Some of the people chosen can't approve changes. Choose again.");
        }

        return chosen;
    }
}
