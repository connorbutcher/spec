using PUSpecSheet.Application.Common;

namespace PUSpecSheet.Application.Sheets.Reading;

/// <summary>
/// Which moment of a sheet to show: the live state (nothing set), the state when a version was published,
/// the state at a date and time, or the live state with a submission's changes on top, for its approvers.
/// </summary>
public sealed record SheetViewPoint
{
    public SheetViewPoint(int? versionNumber, DateTime? asOfUtc)
    {
        this.VersionNumber = versionNumber;
        this.AsOfUtc = asOfUtc;
    }

    public int? VersionNumber { get; init; }

    public DateTime? AsOfUtc { get; init; }

    public static SheetViewPoint Live { get; } = new(null, null);

    /// <summary>The submission whose waiting changes are shown over the published sheet, read-only.</summary>
    public int? ReviewedSubmissionId { get; init; }

    public bool IsLive => VersionNumber is null && AsOfUtc is null && ReviewedSubmissionId is null;

    /// <summary>Builds a view point from the two query options, which can't be used together.</summary>
    public static SheetViewPoint From(int? versionNumber, DateTime? asOf)
    {
        return From(versionNumber, asOf, null);
    }

    /// <summary>Builds a view point from the three query options, of which at most one can be used.</summary>
    public static SheetViewPoint From(int? versionNumber, DateTime? asOf, int? submissionId)
    {
        var given = (versionNumber is null ? 0 : 1) + (asOf is null ? 0 : 1) + (submissionId is null ? 0 : 1);
        if (given > 1)
        {
            throw new InvalidRequestException("Ask for a version number, a date or a submission, not more than one.");
        }

        return new SheetViewPoint(versionNumber, asOf is { } moment ? ToUtc(moment) : null)
        {
            ReviewedSubmissionId = submissionId,
        };
    }

    private static DateTime ToUtc(DateTime moment)
    {
        return moment.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(moment, DateTimeKind.Utc)
            : moment.ToUniversalTime();
    }
}
