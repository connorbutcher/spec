using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Editing;

public interface ISheetColumnBlocksService
{
    Task<Sheet> AddAsync(int tableId, AddSheetColumnBlockRequest request, CancellationToken cancellationToken);

    Task<Sheet> MoveAsync(int columnBlockId, MoveRequest request, CancellationToken cancellationToken);

    // Removes a column block copy, with its cells in every row, never below the template's minimum.
    Task<Sheet> RemoveAsync(int columnBlockId, CancellationToken cancellationToken);
}
