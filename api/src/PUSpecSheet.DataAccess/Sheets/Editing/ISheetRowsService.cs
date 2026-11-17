using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Editing;

public interface ISheetRowsService
{
    Task<Sheet> SaveValuesAsync(int rowId, SaveRowValuesRequest request, CancellationToken cancellationToken);

    Task<Sheet> MoveAsync(int rowId, MoveRequest request, CancellationToken cancellationToken);

    Task<Sheet> RemoveAsync(int rowId, CancellationToken cancellationToken);

    Task<Sheet> SaveCellSettingsAsync(int rowId, SaveRowCellSettingsRequest request, CancellationToken cancellationToken);
}
