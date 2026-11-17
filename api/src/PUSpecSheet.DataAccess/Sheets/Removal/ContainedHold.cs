namespace PUSpecSheet.DataAccess.Sheets.Removal;

public sealed record ContainedHold
{
    public required int UserId { get; init; }

    public required bool AwaitingApproval { get; init; }
}
