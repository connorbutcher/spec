using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// A workflow step as a submission. It is saved after the sheet, as it can name the version it published, and then
/// its revisions are pointed at it.
/// </summary>
internal sealed record PlannedSubmission
{
    public required DbSheetSubmission Submission { get; init; }

    public required DbSheetVersion? Version { get; init; }

    public required IReadOnlyList<IDbSheetRevision> Revisions { get; init; }
}
