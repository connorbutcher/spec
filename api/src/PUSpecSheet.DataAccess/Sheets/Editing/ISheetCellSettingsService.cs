using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Editing;

// The settings of a cell that are chosen on the sheet, not in the template: where a linked dropdown takes its choices
// from, for example. They are part of the row, so changing them starts the current user's draft on it exactly as
// changing a value does.
public interface ISheetCellSettingsService
{
    Task<Sheet> SaveAsync(int rowId, SaveRowCellSettingsRequest request, CancellationToken cancellationToken);
}
