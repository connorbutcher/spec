namespace PUSpecSheet.DataAccess.Sheets.Removal;

/// <summary>A draft that removes a section or table, which nobody has published yet.</summary>
public sealed record PendingRemoval
{
    public required RemovedContainer Container { get; init; }

    public required int AuthorUserId { get; init; }

    public required bool AwaitingApproval { get; init; }
}
