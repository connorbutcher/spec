using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets;

namespace PUSpecSheet.Application.Tests.Sheets;

public sealed class SheetViewPointTests
{
    [Fact]
    public void NeitherOption_IsTheLiveSheet()
    {
        var view = SheetViewPoint.From(null, null);

        Assert.True(view.IsLive);
        Assert.Equal(SheetViewPoint.Live, view);
    }

    [Fact]
    public void AVersionNumber_IsAPastView()
    {
        var view = SheetViewPoint.From(3, null);

        Assert.False(view.IsLive);
        Assert.Equal(3, view.VersionNumber);
    }

    [Fact]
    public void ADateWithoutAZone_IsTakenAsUtc()
    {
        var view = SheetViewPoint.From(null, new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Unspecified));

        Assert.Equal(new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc), view.AsOfUtc);
        Assert.Equal(DateTimeKind.Utc, view.AsOfUtc!.Value.Kind);
    }

    [Fact]
    public void ALocalDate_IsConvertedToUtc()
    {
        var local = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Local);

        var view = SheetViewPoint.From(null, local);

        Assert.Equal(local.ToUniversalTime(), view.AsOfUtc);
    }

    [Fact]
    public void AVersionAndADateTogether_AreRefused()
    {
        Assert.Throws<InvalidRequestException>(() => SheetViewPoint.From(3, DateTime.UtcNow));
    }
}
