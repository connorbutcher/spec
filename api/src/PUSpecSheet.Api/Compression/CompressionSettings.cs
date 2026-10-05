using System.IO.Compression;

namespace PUSpecSheet.Api.Compression;

/// <summary>The "Compression" configuration section: how API responses are compressed.</summary>
public sealed class CompressionSettings
{
    public const string SectionName = "Compression";

    /// <summary>Turns response compression on or off for every response.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// How hard to squeeze. <see cref="CompressionLevel.Fastest"/> is the right choice for responses built per
    /// request: nearly all of the saving for a fraction of the CPU of the stronger levels.
    /// </summary>
    public CompressionLevel Level { get; init; } = CompressionLevel.Fastest;

    /// <summary>
    /// Whether to compress responses sent over HTTPS. Compressing secrets that sit next to text an attacker
    /// can influence can leak them through the response size (the BREACH attack). This API's responses hold
    /// no tokens or other secrets, so it is on; turn it off if that ever changes.
    /// </summary>
    public bool EnableForHttps { get; init; } = true;
}
