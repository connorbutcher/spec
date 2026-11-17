using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets;

public interface ISheetRowDataService
{
    Task<List<DbSheetRow>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken);

    Task<DbSheetRow?> FindWithCellTypesAsync(int id, bool withOptions, CancellationToken cancellationToken);

    Task<DbSheetRow?> FindWithSectionAsync(int id, CancellationToken cancellationToken);

    Task<int?> FindSheetIdAsync(int id, CancellationToken cancellationToken);

    Task<int?> FindSectionIdAsync(int id, CancellationToken cancellationToken);

    Task<SheetRowPlace?> FindPlaceAsync(int id, CancellationToken cancellationToken);

    Task<List<int>> ListIdsInSectionAsync(int sectionId, CancellationToken cancellationToken);

    Task<List<int>> ListIdsInSectionsAsync(IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken);

    Task<List<int>> ListIdsOnTableAsync(int tableId, CancellationToken cancellationToken);

    Task<List<int>> ListTemplateRowIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    Task<Dictionary<int, int>> GetTemplateRowsOnTableAsync(int tableId, IReadOnlyCollection<int> templateRowIds, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);

    Task<List<DbSheetRowRevision>> ListVisibleInSectionAsync(int sectionId, int userId, CancellationToken cancellationToken);

    Task<List<DbSheetRowRevision>> ListVisibleAsync(IReadOnlyCollection<int> rowIds, int userId, CancellationToken cancellationToken);

    Task<List<DbSheetRowRevision>> ListCurrentAndDraftAsync(int rowId, CancellationToken cancellationToken);

    Task<DbSheetRowRevision?> FindDraftForEditAsync(int rowId, CancellationToken cancellationToken);

    Task<int?> FindDraftAuthorAsync(int rowId, CancellationToken cancellationToken);

    Task<List<ContainedHold>> ListHoldsInSectionsAsync(IReadOnlyCollection<int> sectionIds, int userId, CancellationToken cancellationToken);

    Task<List<ContainedHold>> ListHoldsOnTableAsync(int tableId, int userId, CancellationToken cancellationToken);
}
