namespace PUSpecSheet.Contracts.Sheets;

/// <summary>Who holds a draft on a table, section or row. While it exists, only that person can change the item.</summary>
public sealed record SheetLockDto
{
    public SheetLockDto(int userId, string userName, bool isMine)
    {
        this.UserId = userId;
        this.UserName = userName;
        this.IsMine = isMine;
    }

    public int UserId { get; init; }

    public string UserName { get; init; }

    public bool IsMine { get; init; }

    /// <summary>
    /// The draft has been submitted for publishing and waits for approval. Nobody can change the item until
    /// it is approved (and published) or rejected (and back with its author), the author included.
    /// </summary>
    public bool IsAwaitingApproval { get; init; }
}
