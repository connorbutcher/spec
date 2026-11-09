namespace PUSpecSheet.Application.Sheets.Removal;

/// <summary>
/// Something inside a section or table that someone holds: a draft they saved, or a row they have clicked
/// into. A draft waiting for approval is frozen whoever submitted it.
/// </summary>
public sealed record ContainedHold(int UserId, bool AwaitingApproval);
