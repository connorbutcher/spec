using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Abstractions.Requests.Sheets;

/// <summary>
/// The current user's decision on the sheet's pending submission.
/// </summary>
public sealed record DecideSubmissionRequest
{
    public required ApproverDecision Decision { get; init; }

    /// <summary>
    /// Why; required when rejecting, as it is sent to who submitted the draft.
    /// </summary>
    public string? Comment { get; init; }
}
