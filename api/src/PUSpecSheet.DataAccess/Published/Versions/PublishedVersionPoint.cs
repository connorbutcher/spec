namespace PUSpecSheet.DataAccess.Published.Versions;

public sealed record PublishedVersionPoint
{
    public int? VersionNumber { get; init; }

    public DateTime? AtUtc { get; init; }

    public static PublishedVersionPoint Latest { get; } = new PublishedVersionPoint();

    public static PublishedVersionPoint Version(int versionNumber)
    {
        return new PublishedVersionPoint { VersionNumber = versionNumber };
    }

    public static PublishedVersionPoint At(DateTime momentUtc)
    {
        return new PublishedVersionPoint { AtUtc = momentUtc };
    }
}
