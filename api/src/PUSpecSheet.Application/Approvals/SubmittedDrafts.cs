using PUSpecSheet.Application.Persistence.Sheets;

namespace PUSpecSheet.Application.Approvals;

/// <summary>Hands a submission's drafts back to their authors, as open drafts they can change, publish or discard again.</summary>
internal static class SubmittedDrafts
{
    public static async Task ReleaseAsync(SheetRevisionRepositories revisions, int sheetId, int submissionId, CancellationToken cancellationToken)
    {
        await revisions.Tables.ReleaseSubmissionAsync(sheetId, submissionId, cancellationToken);
        await revisions.Sections.ReleaseSubmissionAsync(sheetId, submissionId, cancellationToken);
        await revisions.Rows.ReleaseSubmissionAsync(sheetId, submissionId, cancellationToken);
        await revisions.ColumnBlocks.ReleaseSubmissionAsync(sheetId, submissionId, cancellationToken);
    }
}
