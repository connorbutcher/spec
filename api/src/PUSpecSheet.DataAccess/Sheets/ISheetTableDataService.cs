using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets;

public interface ISheetTableDataService
{
    Task<List<DbSheetTable>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken);

    Task<DbSheetTable?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<DbSheetTable?> FindWithTemplateVersionAsync(int id, CancellationToken cancellationToken);

    Task<int?> FindSheetIdAsync(int id, CancellationToken cancellationToken);

    Task<List<int>> ListIdsOnSheetAsync(int sheetId, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);

    Task<List<DbSheetTableRevision>> ListVisibleOnSheetAsync(int sheetId, int userId, CancellationToken cancellationToken);

    Task<List<PendingRemoval>> ListPendingRemovalsAsync(int tableId, CancellationToken cancellationToken);
}
