using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Approvals;

/// <summary>Rejects a submission, saying why. The reason is shown to the submitter.</summary>
public sealed record RejectSubmissionRequest([Required, StringLength(1000, MinimumLength = 1)] string Reason);
