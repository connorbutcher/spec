using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets.Editing;

/// <summary>Changes to the tables on a sheet. Every action returns the refreshed live view of the sheet.</summary>
public interface ISheetTablesService
{
    /// <summary>Adds a table from the latest version of a template, with its header and starting sections, as a draft.</summary>
    Task<SheetDto> AddAsync(int sheetId, AddSheetTableRequest request, CancellationToken cancellationToken);

    Task<SheetDto> SetTitleAsync(int tableId, UpdateSheetTableRequest request, CancellationToken cancellationToken);

    Task<SheetDto> MoveAsync(int tableId, MoveRequest request, CancellationToken cancellationToken);

    /// <summary>Removes a table. A table nobody has published is simply deleted.</summary>
    Task<SheetDto> RemoveAsync(int tableId, CancellationToken cancellationToken);

    /// <summary>Adds a copy of an addable section (with its rows and starting sub-sections) within the template's limits.</summary>
    Task<SheetDto> AddSectionAsync(int tableId, AddSheetSectionRequest request, CancellationToken cancellationToken);
}
