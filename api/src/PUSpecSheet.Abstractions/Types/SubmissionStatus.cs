namespace PUSpecSheet.Abstractions.Types;

/// <summary>
/// Where a submission of a sheet's drafts is in its approval.
/// </summary>
public enum SubmissionStatus
{
    Pending,
    Approved,
    Rejected,
    Withdrawn
}
