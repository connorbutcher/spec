using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Query.Requests;

public sealed record DatasetRequest
{
    [Range(1, byte.MaxValue)]
    public required int PUSpecSheetTypeId { get; init; }

    [Range(1, short.MaxValue)]
    public required int PUSpecSheetPhaseId { get; init; }

    public required bool UseLatestVersion { get; init; }

    /// <summary>
    /// This will be null when UseLatestVersion is true, otherwise it will be the ID of the PUSpecSheet to use.
    /// </summary>
    public int? PUSpecSheetId { get; init; }

    public required Guid[] ReferenceIds { get; init; }
}
