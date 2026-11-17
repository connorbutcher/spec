namespace PUSpecSheet.DataAccess.Published.Versions;

/// <summary>
/// Which published version of a sheet a caller wants: a version number, the version that was current at a
/// moment, or (neither set) the newest.
/// </summary>
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
