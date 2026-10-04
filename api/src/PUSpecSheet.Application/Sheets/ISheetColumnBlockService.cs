using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Changes to the column block copies on a horizontal sheet table. Every action returns the refreshed live view of the sheet.</summary>
public interface ISheetColumnBlockService
{
    /// <summary>
    /// Adds a copy of a template column block to the right of the existing ones, within the template's
    /// limits. Every row of the table gets its cells in it.
    /// </summary>
    Task<SheetDto> AddAsync(int tableId, AddSheetColumnBlockRequest request, CancellationToken cancellationToken);

    Task<SheetDto> MoveAsync(int columnBlockId, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Removes a column block copy, with its cells in every row, never below the template's minimum.</summary>
    Task<SheetDto> RemoveAsync(int columnBlockId, CancellationToken cancellationToken);
}
