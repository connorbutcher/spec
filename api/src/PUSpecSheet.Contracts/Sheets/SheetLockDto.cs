namespace PUSpecSheet.Contracts.Sheets;

/// <summary>Who holds a draft on a table, section or row. While it exists, only that person can change the item.</summary>
public sealed record SheetLockDto(int UserId, string UserName, bool IsMine);
