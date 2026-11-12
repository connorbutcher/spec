namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// Whether the person asking could request a takeover of a row right now and, when they couldn't, why
/// not in words to show them: the row has unpublished changes, or someone else has already asked.
/// </summary>
public sealed record RowTakeoverAvailabilityDto
{
    public RowTakeoverAvailabilityDto(bool isAvailable, string? reason)
    {
        this.IsAvailable = isAvailable;
        this.Reason = reason;
    }

    public bool IsAvailable { get; init; }

    public string? Reason { get; init; }
}
