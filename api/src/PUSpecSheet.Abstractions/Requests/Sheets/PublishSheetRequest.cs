namespace PUSpecSheet.Abstractions.Requests.Sheets;

/// <summary>
/// Submits the current user's checked-out changes to the sheet: publishes them as its next version, or sends them to the
/// approvers when its type needs approval.
/// </summary>
public sealed record PublishSheetRequest
{
    public required string ChangeLog { get; init; }

    /// <summary>
    /// The users who must approve the changes (not the submitter); ignored for a sheet type that doesn't need approval.
    /// </summary>
    public required ICollection<int> ApproverUserIds { get; init; }
}
