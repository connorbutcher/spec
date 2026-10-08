namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// A row someone has clicked into but not changed yet. It is theirs while they stay in it; nothing is
/// saved, so it is not part of the sheet itself and is sent alongside it. Once they change the row, the
/// sheet shows it locked to them as usual.
/// </summary>
public sealed record RowCheckoutDto(int RowId, int UserId, string UserName);
