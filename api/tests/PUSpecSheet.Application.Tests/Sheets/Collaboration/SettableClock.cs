namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

/// <summary>A clock a test moves forward by hand.</summary>
internal sealed class SettableClock : TimeProvider
{
    private DateTimeOffset now = new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow()
    {
        return now;
    }

    public void Advance(TimeSpan time)
    {
        now += time;
    }
}
