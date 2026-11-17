namespace PUSpecSheet.DataAccess.Sheets.Removal;

public sealed record PendingRemoval
{
    public required RemovedContainer Container { get; init; }

    public required int AuthorUserId { get; init; }

    public required bool AwaitingApproval { get; init; }
}
