using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Changes to the section copies on a sheet table. Every action returns the refreshed live view of the sheet.</summary>
public interface ISheetSectionService
{
    /// <summary>Adds a copy of an addable section (with its rows and starting sub-sections) within the template's limits.</summary>
    Task<SheetDto> AddAsync(int tableId, AddSheetSectionRequest request, CancellationToken cancellationToken);

    Task<SheetDto> MoveAsync(int sectionId, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Removes a section copy and everything in it, never below the template's minimum and never the header.</summary>
    Task<SheetDto> RemoveAsync(int sectionId, CancellationToken cancellationToken);
}
