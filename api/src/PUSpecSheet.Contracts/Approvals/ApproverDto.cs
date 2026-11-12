namespace PUSpecSheet.Contracts.Approvals;

/// <summary>Someone who can be asked to approve a submission.</summary>
public sealed record ApproverDto
{
    public ApproverDto(int userId, string userName)
    {
        this.UserId = userId;
        this.UserName = userName;
    }

    public int UserId { get; init; }

    public string UserName { get; init; }
}
