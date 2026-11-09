using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Approvals;

/// <summary>Hands a submission's drafts back to their authors, as open drafts they can change, publish or discard again.</summary>
internal static class SubmittedDrafts
{
    public static async Task ReleaseAsync(PuSpecSheetDbContext db, int sheetId, int submissionId, CancellationToken cancellationToken)
    {
        await ReleaseAsync(db.TableRevisionsOf(sheetId), submissionId, cancellationToken);
        await ReleaseAsync(db.SectionRevisionsOf(sheetId), submissionId, cancellationToken);
        await ReleaseAsync(db.RowRevisionsOf(sheetId), submissionId, cancellationToken);
        await ReleaseAsync(db.ColumnBlockRevisionsOf(sheetId), submissionId, cancellationToken);
    }

    private static async Task ReleaseAsync<TRevision>(IQueryable<TRevision> revisions, int submissionId, CancellationToken cancellationToken)
        where TRevision : class, ISheetRevision
    {
        await revisions.InSubmission(submissionId).ExecuteUpdateAsync(
            setters => setters.SetProperty(revision => revision.SheetSubmissionId, (int?)null),
            cancellationToken);
    }
}
