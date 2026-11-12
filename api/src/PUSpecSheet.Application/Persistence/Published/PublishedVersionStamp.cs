namespace PUSpecSheet.Application.Persistence.Published;

/// <summary>A published version of a sheet: its number and when it was published.</summary>
public sealed record PublishedVersionStamp(int SheetId, int VersionNumber, DateTime PublishedAtUtc);
