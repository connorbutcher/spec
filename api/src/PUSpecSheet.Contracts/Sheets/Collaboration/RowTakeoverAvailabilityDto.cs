namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// Whether the person asking could request a takeover of a row right now and, when they couldn't, why
/// not in words to show them: the row has unpublished changes, or someone else has already asked.
/// </summary>
public sealed record RowTakeoverAvailabilityDto(bool IsAvailable, string? Reason);
