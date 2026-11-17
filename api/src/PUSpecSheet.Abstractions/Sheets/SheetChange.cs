namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>The published version in which something on a sheet last changed, and who changed it.</summary>
public sealed record SheetChange
{
    public required int VersionNumber { get; init; }

    public required DateTime AtUtc { get; init; }

    public required string UserName { get; init; }
}
