namespace PUSpecSheet.Application.Published;

/// <summary>
/// Which published version of a sheet a caller wants: a version number, the version that was current at a
/// moment, or (neither set) the newest.
/// </summary>
public sealed record PublishedVersionPoint(int? VersionNumber, DateTime? AtUtc)
{
    public static PublishedVersionPoint Latest { get; } = new(null, null);

    public static PublishedVersionPoint Version(int versionNumber)
    {
        return new PublishedVersionPoint(versionNumber, null);
    }

    public static PublishedVersionPoint At(DateTime momentUtc)
    {
        return new PublishedVersionPoint(null, momentUtc);
    }
}
