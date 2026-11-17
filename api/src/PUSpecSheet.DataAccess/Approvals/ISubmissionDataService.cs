using PUSpecSheet.Database.Models.Approvals;

namespace PUSpecSheet.DataAccess.Approvals;

public interface ISubmissionDataService
{
    Task<List<DbSheetSubmission>> ListAwaitingDecisionOfAsync(int userId, CancellationToken cancellationToken);

    Task<List<DbSheetSubmission>> ListSubmittedByAsync(int userId, DateTime decidedSince, CancellationToken cancellationToken);

    Task<List<DbSheetSubmission>> ListByIdAsync(int id, CancellationToken cancellationToken);

    Task<int?> FindSheetIdAsync(int id, CancellationToken cancellationToken);

    Task<SubmissionReviewAccess?> FindReviewAccessAsync(int sheetId, int id, int userId, CancellationToken cancellationToken);

    Task<DbSheetSubmission?> FindWithApproversForEditAsync(int id, CancellationToken cancellationToken);
}
