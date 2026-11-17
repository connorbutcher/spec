namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;

public sealed record RowTakeoverOptions
{
    public const string SectionName = "RowTakeover";

    public const int MinimumResponseSeconds = 5;

    public int ResponseSeconds { get; init; } = 60;

    public TimeSpan ResponseTime => TimeSpan.FromSeconds(Math.Max(ResponseSeconds, MinimumResponseSeconds));
}
