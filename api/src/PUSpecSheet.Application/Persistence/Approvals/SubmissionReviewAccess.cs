using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Application.Persistence.Approvals;

/// <summary>What decides whether someone may review a submission: its state, its submitter, and whether they were asked to approve it.</summary>
public sealed record SubmissionReviewAccess(SubmissionStatus Status, int SubmittedByUserId, bool IsApprover);
