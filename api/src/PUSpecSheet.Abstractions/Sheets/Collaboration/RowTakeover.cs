namespace PUSpecSheet.Abstractions.Sheets.Collaboration;

/// <summary>
/// One person asking to take over a row that is checked out to another. While it is
/// <see cref="RowTakeoverStatus.Pending"/> the holder can approve or deny it and the requester can cancel
/// it; at <see cref="ExpiresAtUtc"/> an unanswered request is granted. Only a row the holder has not
/// changed can change hands, so a takeover never moves anyone's unpublished work.
/// </summary>
public sealed record RowTakeover
{
    public required Guid Id { get; init; }

    public required int SheetId { get; init; }

    public required int RowId { get; init; }

    public required int RequesterUserId { get; init; }

    public required string RequesterName { get; init; }

    public required int HolderUserId { get; init; }

    public required string HolderName { get; init; }

    public required DateTime RequestedAtUtc { get; init; }

    public required DateTime ExpiresAtUtc { get; init; }

    public required RowTakeoverStatus Status { get; init; }
}
