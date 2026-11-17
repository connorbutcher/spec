using PUSpecSheet.Database.Models.Approvals;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Migrator.Building;

// A legacy workflow step as a submission. It is saved after the sheet, as it can name the version it published, and
// then its revisions are pointed at it.
internal sealed record PlannedSubmission
{
    public required DbSheetSubmission Submission { get; init; }

    public DbSheetVersion? Version { get; init; }

    public IReadOnlyList<IDbSheetRevision> Revisions { get; init; } = [];
}
