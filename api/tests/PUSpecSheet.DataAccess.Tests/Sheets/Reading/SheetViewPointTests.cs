using PUSpecSheet.DataAccess.Sheets.Reading;

namespace PUSpecSheet.DataAccess.Tests.Sheets.Reading;

public sealed class SheetViewPointTests
{
    [Fact]
    public void TheLiveView_IsLive()
    {
        Assert.True(SheetViewPoint.Live.IsLive);
    }

    [Fact]
    public void AVersionNumber_IsAPastView()
    {
        var view = SheetViewPoint.AtVersion(3);

        Assert.False(view.IsLive);
        Assert.Equal(3, view.VersionNumber);
        Assert.Null(view.AsOfUtc);
    }

    [Fact]
    public void ADateWithoutAZone_IsTakenAsUtc()
    {
        var view = SheetViewPoint.AsOf(new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Unspecified));

        Assert.False(view.IsLive);
        Assert.Equal(new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc), view.AsOfUtc);
        Assert.Equal(DateTimeKind.Utc, view.AsOfUtc!.Value.Kind);
    }

    [Fact]
    public void ALocalDate_IsConvertedToUtc()
    {
        var local = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Local);

        var view = SheetViewPoint.AsOf(local);

        Assert.Equal(local.ToUniversalTime(), view.AsOfUtc);
    }

    [Fact]
    public void ReviewingASubmission_IsNotTheLiveView()
    {
        var view = SheetViewPoint.Reviewing(12);

        Assert.False(view.IsLive);
        Assert.Equal(12, view.ReviewedSubmissionId);
        Assert.Null(view.VersionNumber);
        Assert.Null(view.AsOfUtc);
    }
}
