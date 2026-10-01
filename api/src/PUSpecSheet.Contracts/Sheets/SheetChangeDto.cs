namespace PUSpecSheet.Contracts.Sheets;

/// <summary>The published version in which something on a sheet last changed, and who changed it.</summary>
public sealed record SheetChangeDto(int VersionNumber, DateTime AtUtc, string UserName);
