using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets;

public interface ISheetColumnBlockDataService
{
    Task<List<DbSheetColumnBlock>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken);

    Task<DbSheetColumnBlock?> FindWithTemplateAndTableAsync(int id, CancellationToken cancellationToken);

    Task<List<SheetColumnBlockLink>> ListLinksOnTableAsync(int tableId, CancellationToken cancellationToken);

    Task<List<int>> ListIdsOnTableAsync(int tableId, CancellationToken cancellationToken);

    Task<List<DbSheetColumnBlockRevision>> ListVisibleOnTableAsync(int tableId, int userId, CancellationToken cancellationToken);

    Task<List<ContainedHold>> ListHoldsOnTableAsync(int tableId, int userId, CancellationToken cancellationToken);

    Task DeleteAsync(IReadOnlyCollection<int> blockIds, CancellationToken cancellationToken);
}
