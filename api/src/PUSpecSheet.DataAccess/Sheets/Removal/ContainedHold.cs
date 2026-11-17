namespace PUSpecSheet.DataAccess.Sheets.Removal;

/// <summary>
/// Something inside a section or table that someone holds: a draft they saved, or a row they have clicked
/// into. A draft waiting for approval is frozen whoever submitted it.
/// </summary>
public sealed record ContainedHold
{
    public required int UserId { get; init; }

    public required bool AwaitingApproval { get; init; }
}
