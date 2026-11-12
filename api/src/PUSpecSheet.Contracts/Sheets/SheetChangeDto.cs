namespace PUSpecSheet.Contracts.Sheets;

/// <summary>The published version in which something on a sheet last changed, and who changed it.</summary>
public sealed record SheetChangeDto
{
    public SheetChangeDto(int versionNumber, DateTime atUtc, string userName)
    {
        this.VersionNumber = versionNumber;
        this.AtUtc = atUtc;
        this.UserName = userName;
    }

    public int VersionNumber { get; init; }

    public DateTime AtUtc { get; init; }

    public string UserName { get; init; }
}
