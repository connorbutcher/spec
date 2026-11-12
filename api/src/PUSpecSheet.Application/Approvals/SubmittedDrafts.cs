using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Approvals;

/// <summary>Hands a submission's drafts back to their authors, as open drafts they can change, publish or discard again.</summary>
internal static class SubmittedDrafts
{
    public static async Task ReleaseAsync(PuSpecSheetDbContext db, int sheetId, int submissionId, CancellationToken cancellationToken)
    {
        await db.SheetTableRevisions.ReleaseSubmissionAsync(sheetId, submissionId, cancellationToken);
        await db.SheetSectionRevisions.ReleaseSubmissionAsync(sheetId, submissionId, cancellationToken);
        await db.SheetRowRevisions.ReleaseSubmissionAsync(sheetId, submissionId, cancellationToken);
        await db.SheetColumnBlockRevisions.ReleaseSubmissionAsync(sheetId, submissionId, cancellationToken);
    }
}
