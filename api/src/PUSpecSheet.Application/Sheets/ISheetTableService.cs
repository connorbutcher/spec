using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Changes to the tables on a sheet. Every action returns the refreshed live view of the sheet.</summary>
public interface ISheetTableService
{
    /// <summary>Adds a table from the latest version of a template, with its header and starting sections, as a draft.</summary>
    Task<SheetDto> AddAsync(int sheetId, AddSheetTableRequest request, CancellationToken cancellationToken);

    Task<SheetDto> SetTitleAsync(int tableId, UpdateSheetTableRequest request, CancellationToken cancellationToken);

    Task<SheetDto> MoveAsync(int tableId, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Removes a table. A table nobody has published is simply deleted.</summary>
    Task<SheetDto> RemoveAsync(int tableId, CancellationToken cancellationToken);
}
