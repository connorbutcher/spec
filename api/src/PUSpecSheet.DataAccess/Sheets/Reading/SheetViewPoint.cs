namespace PUSpecSheet.DataAccess.Sheets.Reading;

public sealed record SheetViewPoint
{
    private SheetViewPoint()
    {
    }

    public static SheetViewPoint Live { get; } = new();

    public int? VersionNumber { get; private init; }

    public DateTime? AsOfUtc { get; private init; }

    public int? ReviewedSubmissionId { get; private init; }

    public bool IsLive => VersionNumber is null && AsOfUtc is null && ReviewedSubmissionId is null;

    public static SheetViewPoint AtVersion(int versionNumber)
    {
        return new SheetViewPoint { VersionNumber = versionNumber };
    }

    public static SheetViewPoint AsOf(DateTime moment)
    {
        return new SheetViewPoint { AsOfUtc = ToUtc(moment) };
    }

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
