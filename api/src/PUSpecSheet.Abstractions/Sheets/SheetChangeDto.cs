namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>The published version in which something on a sheet last changed, and who changed it.</summary>
public sealed record SheetChangeDto
{
    public SheetChangeDto(int versionNumber, DateTime atUtc, string userName)
    {
        VersionNumber = versionNumber;
        AtUtc = atUtc;
        UserName = userName;
    }

    public int VersionNumber { get; init; }

    public DateTime AtUtc { get; init; }

    public string UserName { get; init; }
}
