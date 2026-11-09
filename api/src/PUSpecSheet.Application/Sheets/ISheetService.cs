using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets;

public interface ISheetService
{
    /// <summary>
    /// The sheet of a sheet type for a phase, created empty the first time it's opened. Fails if the
    /// sheet type isn't available to the phase.
    /// </summary>
    Task<SheetDto> OpenAsync(int phaseId, int sheetTypeId, SheetViewPoint view, CancellationToken cancellationToken);

    Task<SheetDto> GetAsync(int sheetId, SheetViewPoint view, CancellationToken cancellationToken);

    /// <summary>
    /// Some rows of the live sheet as the current user sees them, with what follows from them. Unlike a
    /// whole read it leaves the user's drafts as they are, so nothing else on the sheet changes under them.
    /// </summary>
    Task<SheetRowsChangeDto> GetRowsAsync(int sheetId, IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken);

    /// <summary>
    /// Publishes drafts on the sheet as its next version straight away, without approval: the current user's
    /// own, or everyone's when the request asks for <see cref="PublishScope.All"/>. Only start-up seeding uses
    /// this; publishing through the API waits for approval (see <c>ISheetSubmissionService</c>).
    /// </summary>
    Task<SheetDto> PublishAsync(int sheetId, PublishSheetRequest request, CancellationToken cancellationToken);

    /// <summary>Throws away all of the current user's drafts on the sheet, releasing their locks.</summary>
    Task<SheetDto> DiscardDraftsAsync(int sheetId, CancellationToken cancellationToken);
}
