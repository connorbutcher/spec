using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Editing;

public interface ISheetSectionsService
{
    Task<Sheet> AddAsync(int tableId, AddSheetSectionRequest request, CancellationToken cancellationToken);

    Task<Sheet> MoveAsync(int sectionId, MoveRequest request, CancellationToken cancellationToken);

    // Removes a section copy and everything in it, never below the template's minimum and never the header.
    Task<Sheet> RemoveAsync(int sectionId, CancellationToken cancellationToken);
}
