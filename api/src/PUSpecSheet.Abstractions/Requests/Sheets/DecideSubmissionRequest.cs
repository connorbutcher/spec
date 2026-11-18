using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Abstractions.Requests.Sheets;

/// <summary>
/// The current user's decision on one of the sheet's pending submissions.
/// </summary>
public sealed record DecideSubmissionRequest
{
    public required ApproverDecision Decision { get; init; }

    /// <summary>
    /// Why; required when rejecting, as it is sent to who submitted the changes.
    /// </summary>
    public string? Comment { get; init; }
}
