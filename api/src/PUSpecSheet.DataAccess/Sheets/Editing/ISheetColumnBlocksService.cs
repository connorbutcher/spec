using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Editing;

/// <summary>Changes to the column block copies on a horizontal sheet table. Every action returns the refreshed live view of the sheet.</summary>
public interface ISheetColumnBlocksService
{
    /// <summary>
    /// Adds a copy of a template column block to the right of the existing ones, within the template's
    /// limits. Every row of the table gets its cells in it.
    /// </summary>
    Task<Sheet> AddAsync(int tableId, AddSheetColumnBlockRequest request, CancellationToken cancellationToken);

    Task<Sheet> MoveAsync(int columnBlockId, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Removes a column block copy, with its cells in every row, never below the template's minimum.</summary>
    Task<Sheet> RemoveAsync(int columnBlockId, CancellationToken cancellationToken);
}
