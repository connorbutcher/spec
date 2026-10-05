namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// Everywhere a value was found in a cell with a lookup key, each with the data that belongs to it. Empty
/// <see cref="Matches"/> means the value isn't on any published sheet that was searched.
/// </summary>
public sealed record PublishedLookupDto(
    string Key,
    string Value,
    IReadOnlyList<PublishedLookupMatchDto> Matches);
