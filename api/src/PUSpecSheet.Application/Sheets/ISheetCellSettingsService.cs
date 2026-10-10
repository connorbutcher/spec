using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// The settings of a cell that are chosen on the sheet, not in the template: where a linked dropdown takes
/// its choices from, for example. They are part of the row, so changing them starts the current user's
/// draft on it exactly as changing a value does.
/// </summary>
public interface ISheetCellSettingsService
{
    /// <summary>
    /// Sets or clears the settings of cells in the row, locking it to the current user. A cell whose
    /// settings really change loses its value, which was chosen under the old ones.
    /// </summary>
    Task<SheetDto> SaveAsync(int rowId, SaveRowCellSettingsRequest request, CancellationToken cancellationToken);
}
