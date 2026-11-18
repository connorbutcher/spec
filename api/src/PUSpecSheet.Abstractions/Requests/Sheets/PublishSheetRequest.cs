namespace PUSpecSheet.Abstractions.Requests.Sheets;

/// <summary>
/// Publishes the sheet's draft as its next version, or submits it to the approvers when its type needs approval.
/// </summary>
public sealed record PublishSheetRequest
{
    public required string ChangeLog { get; init; }

    /// <summary>
    /// The users who must approve the draft; ignored for a sheet type that doesn't need approval.
    /// </summary>
    public required ICollection<int> ApproverUserIds { get; init; }
}
