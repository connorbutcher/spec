namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// One person asking to take over a row that is checked out to another. While it is
/// <see cref="RowTakeoverStatus.Pending"/> the holder can approve or deny it and the requester can cancel
/// it; at <see cref="ExpiresAtUtc"/> an unanswered request is granted. Only a row the holder has not
/// changed can change hands, so a takeover never moves anyone's unpublished work.
/// </summary>
public sealed record RowTakeoverDto(
    Guid Id,
    int SheetId,
    int RowId,
    int RequesterUserId,
    string RequesterName,
    int HolderUserId,
    string HolderName,
    DateTime RequestedAtUtc,
    DateTime ExpiresAtUtc,
    RowTakeoverStatus Status);
