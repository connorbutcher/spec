using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Sheets.Reading;

namespace PUSpecSheet.DataAccess.Sheets;

public interface ISheetsService
{
    Task<Sheet> OpenAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken);

    Task<Sheet> GetAsync(int sheetId, SheetViewPoint view, CancellationToken cancellationToken);

    Task<Sheet> GetForReviewAsync(int submissionId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SheetSummary>> ListAsync(int phaseId, CancellationToken cancellationToken);

    Task<SheetSummary> CreateAsync(int phaseId, CreateSheetRequest request, CancellationToken cancellationToken);

    // Some rows of the live sheet as the current user sees them, with what follows from them. Unlike a whole read it
    // leaves the user's drafts as they are, so nothing else on the sheet changes under them.
    Task<SheetRowsChange> GetRowsAsync(int sheetId, IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken);

    Task<Sheet> PublishAsync(int sheetId, string? note, PublishScope scope, CancellationToken cancellationToken);

    Task<Sheet> AddTableAsync(int sheetId, AddSheetTableRequest request, CancellationToken cancellationToken);

    Task<Sheet> SubmitAsync(int sheetId, SubmitSheetRequest request, CancellationToken cancellationToken);
}
