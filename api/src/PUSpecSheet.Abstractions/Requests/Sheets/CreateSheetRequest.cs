using PUSpecSheet.Abstractions.Requests.SpecSheets.Properties;

namespace PUSpecSheet.Abstractions.Requests.Sheets;

/// <summary>
/// Starts the draft of the sheet for a phase and sheet type, making the sheet if there isn't one yet.
/// </summary>
public sealed record CreateSheetRequest
{
    public required int SpecSheetPhaseId { get; init; }

    public required int SpecSheetTypeId { get; init; }

    public required ICollection<CreateSpecSheetPropertyValueRequest> PropertyValues { get; init; }
}
