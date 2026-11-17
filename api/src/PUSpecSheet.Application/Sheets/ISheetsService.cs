using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets;

public interface ISheetsService
{
    /// <summary>
    /// The live view of the phase's sheet of a sheet type with no identifier values, created empty the first
    /// time it's opened. Fails if the sheet type isn't available to the phase, or if it needs identifiers and
    /// the sheet doesn't exist.
    /// </summary>
    Task<SheetDto> OpenAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken);

    /// <summary>The sheet as the current user sees it from the view point: live, at a version or at a moment.</summary>
    Task<SheetDto> GetAsync(int sheetId, SheetViewPoint view, CancellationToken cancellationToken);

    /// <summary>
    /// The live sheet of a submission with the submission's changes on top, read-only. Only its submitter and
    /// approvers may see it, and only while it waits for approval.
    /// </summary>
    Task<SheetDto> GetForReviewAsync(int submissionId, CancellationToken cancellationToken);

    /// <summary>The phase's sheets, of every sheet type, with where each one's versions and drafts stand.</summary>
    Task<IReadOnlyList<SheetSummaryDto>> ListAsync(int phaseId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a sheet of a sheet type in a phase with its identifier values, optionally starting as a copy of
    /// another sheet of the type. Fails if the phase already has a sheet of the type with the same values.
    /// </summary>
    Task<SheetSummaryDto> CreateAsync(int phaseId, CreateSheetRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Some rows of the live sheet as the current user sees them, with what follows from them. Unlike a
    /// whole read it leaves the user's drafts as they are, so nothing else on the sheet changes under them.
    /// </summary>
    Task<SheetRowsChangeDto> GetRowsAsync(int sheetId, IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken);

    /// <summary>
    /// Publishes drafts on the sheet as its next version straight away, without approval: the current user's
    /// own, or everyone's when <paramref name="scope"/> is <see cref="PublishScope.All"/>. Only start-up seeding uses
    /// this; publishing through the API waits for approval (see <c>ISubmissionsService</c>).
    /// </summary>
    Task<SheetDto> PublishAsync(int sheetId, string? note, PublishScope scope, CancellationToken cancellationToken);

    /// <summary>Adds a table from the latest version of a template, with its header and starting sections, as a draft.</summary>
    Task<SheetDto> AddTableAsync(int sheetId, AddSheetTableRequest request, CancellationToken cancellationToken);

    /// <summary>Submits drafts on the sheet for approval and returns the refreshed live sheet.</summary>
    Task<SheetDto> SubmitAsync(int sheetId, SubmitSheetRequest request, CancellationToken cancellationToken);
}
