using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets;

public interface ISheetSectionDataService
{
    Task<List<DbSheetSection>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken);

    Task<DbSheetSection?> FindWithTemplateAndTableAsync(int id, CancellationToken cancellationToken);

    Task<DbSheetSection?> FindOnTableAsync(int id, int tableId, CancellationToken cancellationToken);

    Task<List<SheetSectionPlace>> ListPlacesOnTableAsync(int tableId, CancellationToken cancellationToken);

    Task<Dictionary<int, int?>> GetParentsOnTableAsync(int tableId, CancellationToken cancellationToken);

    Task<Dictionary<int, int?>> GetParentsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    Task<List<int>> ListSiblingIdsAsync(int tableId, int? parentSectionId, CancellationToken cancellationToken);

    Task DeleteAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    Task<List<DbSheetSectionRevision>> ListVisibleOnTableAsync(int tableId, int userId, CancellationToken cancellationToken);

    Task<List<SectionRemoval>> ListPendingRemovalsOnTableAsync(int tableId, CancellationToken cancellationToken);

    Task<List<ContainedHold>> ListHoldsAsync(IReadOnlyCollection<int> sectionIds, int userId, CancellationToken cancellationToken);

    Task<List<ContainedHold>> ListHoldsOnTableAsync(int tableId, int userId, CancellationToken cancellationToken);
}
