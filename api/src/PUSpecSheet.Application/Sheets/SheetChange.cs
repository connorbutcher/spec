namespace PUSpecSheet.Application.Sheets;

/// <summary>The published version in which something on a sheet last changed, when, and who published it.</summary>
public sealed record SheetChange(int VersionNumber, DateTime AtUtc, int AuthorUserId);
