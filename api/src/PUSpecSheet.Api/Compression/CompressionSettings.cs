using System.IO.Compression;

namespace PUSpecSheet.Api.Compression;

/// <summary>The "Compression" configuration section: how API responses are compressed.</summary>
public sealed record CompressionSettings
{
    public const string SectionName = "Compression";

    public bool Enabled { get; init; } = true;

    /// <summary>
    /// How hard to squeeze. <see cref="CompressionLevel.Optimal"/> (Brotli quality 4, gzip level 6) is the
    /// right choice for responses built per request. At <see cref="CompressionLevel.Fastest"/> Brotli, which
    /// browsers prefer, sent a 3 MB sheet as 407 kB against 92 kB at Optimal, for about 10 ms more CPU.
    /// </summary>
    public CompressionLevel Level { get; init; } = CompressionLevel.Optimal;

    /// <summary>
    /// Whether to compress responses sent over HTTPS. Compressing secrets that sit next to text an attacker
    /// can influence can leak them through the response size (the BREACH attack). This API's responses hold
    /// no tokens or other secrets, so it is on; turn it off if that ever changes.
    /// </summary>
    public bool EnableForHttps { get; init; } = true;
}
