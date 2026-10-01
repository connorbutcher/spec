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

    /// <summary>Publishes all of the current user's drafts on the sheet as its next version.</summary>
    Task<SheetDto> PublishAsync(int sheetId, PublishSheetRequest request, CancellationToken cancellationToken);

    /// <summary>Throws away all of the current user's drafts on the sheet, releasing their locks.</summary>
    Task<SheetDto> DiscardDraftsAsync(int sheetId, CancellationToken cancellationToken);
}
