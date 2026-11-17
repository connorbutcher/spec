using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets.Editing;

/// <summary>
/// Changes to the rows of a sheet. A row is the unit that's locked: saving values, moving or removing a
/// row starts the current user's draft on it, which locks it to them until they publish it or change it back.
/// Every action returns the refreshed live view of the sheet.
/// </summary>
public interface ISheetRowsService
{
    /// <summary>Sets or clears cell values in the row, locking it to the current user.</summary>
    Task<SheetDto> SaveValuesAsync(int rowId, SaveRowValuesRequest request, CancellationToken cancellationToken);

    Task<SheetDto> MoveAsync(int rowId, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Removes a row. A row nobody has published is simply deleted. Not allowed in the header.</summary>
    Task<SheetDto> RemoveAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets or clears the settings of cells in the row, locking it to the current user. A cell whose
    /// settings really change loses its value, which was chosen under the old ones.
    /// </summary>
    Task<SheetDto> SaveCellSettingsAsync(int rowId, SaveRowCellSettingsRequest request, CancellationToken cancellationToken);
}
