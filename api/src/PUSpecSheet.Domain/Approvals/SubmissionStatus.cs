namespace PUSpecSheet.Domain.Approvals;

/// <summary>Where a publish waiting for approval is in its life.</summary>
public enum SubmissionStatus
{
    /// <summary>Waiting for its approvers. Its drafts are frozen and stay out of every published view.</summary>
    Pending = 0,

    /// <summary>Every approver approved, and the drafts were published as a version of the sheet.</summary>
    Approved = 1,

    /// <summary>An approver rejected it. The drafts went back to their authors to change and publish again.</summary>
    Rejected = 2,

    /// <summary>The submitter took it back before it was decided. The drafts went back to their authors.</summary>
    Withdrawn = 3,
}
