namespace PUSpecSheet.Application.Sheets.Removal;

/// <summary>A draft that removes a section or table, which nobody has published yet.</summary>
public sealed record PendingRemoval
{
    public PendingRemoval(RemovedContainer container, int authorUserId, bool awaitingApproval)
    {
        this.Container = container;
        this.AuthorUserId = authorUserId;
        this.AwaitingApproval = awaitingApproval;
    }

    public RemovedContainer Container { get; init; }

    public int AuthorUserId { get; init; }

    public bool AwaitingApproval { get; init; }
}
