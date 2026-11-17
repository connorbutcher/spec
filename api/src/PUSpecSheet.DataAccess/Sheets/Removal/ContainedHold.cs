namespace PUSpecSheet.DataAccess.Sheets.Removal;

/// <summary>
/// Something inside a section or table that someone holds: a draft they saved, or a row they have clicked
/// into. A draft waiting for approval is frozen whoever submitted it.
/// </summary>
public sealed record ContainedHold
{
    public ContainedHold(int userId, bool awaitingApproval)
    {
        UserId = userId;
        AwaitingApproval = awaitingApproval;
    }

    public int UserId { get; init; }

    public bool AwaitingApproval { get; init; }
}
