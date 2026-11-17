namespace PUSpecSheet.Abstractions.Published;

/// <summary>Where to find a sheet's published data: its stable identifier and its newest version, if it has one.</summary>
public sealed record PublishedSheetReference
{
    public required Guid Sheet { get; init; }

    public required string Phase { get; init; }

    public required int SheetType { get; init; }

    public int? LatestVersion { get; init; }

    /// <summary>The sheet's identifier values by the identifier's key, such as <c>serialNumber</c>. Empty for a sheet type without identifiers.</summary>
    public IReadOnlyDictionary<string, string> Identifiers { get; init; } = new Dictionary<string, string>();
}
