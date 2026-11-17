namespace PUSpecSheet.Abstractions.Published;

/// <summary>Where to find a sheet's published data: its stable identifier and its newest version, if it has one.</summary>
public sealed record PublishedSheetReferenceDto
{
    public PublishedSheetReferenceDto(Guid sheet, string phase, int sheetType, int? latestVersion)
    {
        this.Sheet = sheet;
        this.Phase = phase;
        this.SheetType = sheetType;
        this.LatestVersion = latestVersion;
    }

    public Guid Sheet { get; init; }

    public string Phase { get; init; }

    public int SheetType { get; init; }

    public int? LatestVersion { get; init; }

    /// <summary>The sheet's identifier values by the identifier's key, such as <c>serialNumber</c>. Empty for a sheet type without identifiers.</summary>
    public IReadOnlyDictionary<string, string> Identifiers { get; init; } = new Dictionary<string, string>();
}
