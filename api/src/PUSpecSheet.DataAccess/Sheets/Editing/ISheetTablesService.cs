using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Editing;

public interface ISheetTablesService
{
    Task<Sheet> AddAsync(int sheetId, AddSheetTableRequest request, CancellationToken cancellationToken);

    Task<Sheet> SetTitleAsync(int tableId, SetSheetTableTitleRequest request, CancellationToken cancellationToken);

    Task<Sheet> MoveAsync(int tableId, MoveRequest request, CancellationToken cancellationToken);

    Task<Sheet> RemoveAsync(int tableId, CancellationToken cancellationToken);

    Task<Sheet> AddSectionAsync(int tableId, AddSheetSectionRequest request, CancellationToken cancellationToken);
}
