namespace PUSpecSheet.Api.Cors;

/// <summary>The "Cors" configuration section: which browser origins may call the API.</summary>
public sealed class CorsSettings
{
    public const string SectionName = "Cors";

    /// <summary>Exact origins allowed to call the API, e.g. "http://localhost:4200".</summary>
    public string[] AllowedOrigins { get; init; } = [];
}
