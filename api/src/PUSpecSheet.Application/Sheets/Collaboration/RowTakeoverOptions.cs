namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>The "RowTakeover" configuration section.</summary>
public sealed class RowTakeoverOptions
{
    public const string SectionName = "RowTakeover";

    /// <summary>The least time a holder is ever given, whatever is configured: long enough to read the request.</summary>
    public const int MinimumResponseSeconds = 5;

    /// <summary>
    /// How long the person a row is checked out to has to answer a takeover request before it is granted
    /// without them.
    /// </summary>
    public int ResponseSeconds { get; set; } = 60;

    public TimeSpan ResponseTime => TimeSpan.FromSeconds(Math.Max(ResponseSeconds, MinimumResponseSeconds));
}
