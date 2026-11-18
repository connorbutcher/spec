namespace PUSpecSheet.Abstractions.Requests.Sheets;

/// <summary>
/// Changes who may approve the sheet's pending submission.
/// </summary>
public sealed record ReassignSubmissionApproversRequest
{
    public required ICollection<int> AddedUserIds { get; init; }

    public required ICollection<int> RemovedUserIds { get; init; }
}
