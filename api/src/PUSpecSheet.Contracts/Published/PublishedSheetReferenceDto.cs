namespace PUSpecSheet.Contracts.Published;

/// <summary>Where to find a sheet's published data: its stable identifier and its newest version, if it has one.</summary>
public sealed record PublishedSheetReferenceDto(
    Guid Sheet,
    string Phase,
    int SheetType,
    int? LatestVersion);
