namespace PUSpecSheet.Application.Sheets.Reading;

/// <summary>
/// Which moment of a sheet to show: the live state (nothing set), the state when a version was published,
/// the state at a date and time, or the live state with a submission's changes on top, for its approvers.
/// Each has its own factory, as each has its own endpoint; at most one is ever set.
/// </summary>
public sealed record SheetViewPoint
{
    private SheetViewPoint()
    {
    }

    public static SheetViewPoint Live { get; } = new();

    public int? VersionNumber { get; private init; }

    public DateTime? AsOfUtc { get; private init; }

    /// <summary>The submission whose waiting changes are shown over the published sheet, read-only.</summary>
    public int? ReviewedSubmissionId { get; private init; }

    public bool IsLive => VersionNumber is null && AsOfUtc is null && ReviewedSubmissionId is null;

    /// <summary>The sheet as it stood when the version was published.</summary>
    public static SheetViewPoint AtVersion(int versionNumber)
    {
        return new SheetViewPoint { VersionNumber = versionNumber };
    }

    /// <summary>The sheet as it was published at the moment. A moment without a zone is taken as UTC.</summary>
    public static SheetViewPoint AsOf(DateTime moment)
    {
        return new SheetViewPoint { AsOfUtc = ToUtc(moment) };
    }

    /// <summary>The live sheet with the submission's changes, waiting for approval, on top.</summary>
    public static SheetViewPoint Reviewing(int submissionId)
    {
        return new SheetViewPoint { ReviewedSubmissionId = submissionId };
    }

    private static DateTime ToUtc(DateTime moment)
    {
        return moment.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(moment, DateTimeKind.Utc)
            : moment.ToUniversalTime();
    }
}
