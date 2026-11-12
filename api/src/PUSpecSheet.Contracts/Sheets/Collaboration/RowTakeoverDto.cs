namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// One person asking to take over a row that is checked out to another. While it is
/// <see cref="RowTakeoverStatus.Pending"/> the holder can approve or deny it and the requester can cancel
/// it; at <see cref="ExpiresAtUtc"/> an unanswered request is granted. Only a row the holder has not
/// changed can change hands, so a takeover never moves anyone's unpublished work.
/// </summary>
public sealed record RowTakeoverDto
{
    public RowTakeoverDto(
        Guid id,
        int sheetId,
        int rowId,
        int requesterUserId,
        string requesterName,
        int holderUserId,
        string holderName,
        DateTime requestedAtUtc,
        DateTime expiresAtUtc,
        RowTakeoverStatus status)
    {
        this.Id = id;
        this.SheetId = sheetId;
        this.RowId = rowId;
        this.RequesterUserId = requesterUserId;
        this.RequesterName = requesterName;
        this.HolderUserId = holderUserId;
        this.HolderName = holderName;
        this.RequestedAtUtc = requestedAtUtc;
        this.ExpiresAtUtc = expiresAtUtc;
        this.Status = status;
    }

    public Guid Id { get; init; }

    public int SheetId { get; init; }

    public int RowId { get; init; }

    public int RequesterUserId { get; init; }

    public string RequesterName { get; init; }

    public int HolderUserId { get; init; }

    public string HolderName { get; init; }

    public DateTime RequestedAtUtc { get; init; }

    public DateTime ExpiresAtUtc { get; init; }

    public RowTakeoverStatus Status { get; init; }
}
