using PUSpecSheet.Application.Common;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Which moment of a sheet to show: the live state (neither set), the state when a version was
/// published, or the state at a date and time.
/// </summary>
public sealed record SheetViewPoint(int? VersionNumber, DateTime? AsOfUtc)
{
    public static SheetViewPoint Live { get; } = new(null, null);

    public bool IsLive => VersionNumber is null && AsOfUtc is null;

    /// <summary>Builds a view point from the two query options, which can't be used together.</summary>
    public static SheetViewPoint From(int? versionNumber, DateTime? asOf)
    {
        if (versionNumber is not null && asOf is not null)
        {
            throw new InvalidRequestException("Ask for a version number or a date, not both.");
        }

        return new SheetViewPoint(versionNumber, asOf is { } moment ? ToUtc(moment) : null);
    }

    private static DateTime ToUtc(DateTime moment)
    {
        return moment.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(moment, DateTimeKind.Utc)
            : moment.ToUniversalTime();
    }
}
