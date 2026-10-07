namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>One live connection (a browser tab) and the sheet it has open.</summary>
public sealed record SheetConnection(int SheetId, int UserId, string DisplayName);
