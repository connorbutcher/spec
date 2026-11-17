using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Approvals;

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
