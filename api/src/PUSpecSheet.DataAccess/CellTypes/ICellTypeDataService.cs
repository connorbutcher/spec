using PUSpecSheet.Database.Models.CellTypes;

namespace PUSpecSheet.DataAccess.CellTypes;

public interface ICellTypeDataService
{
    Task<List<DbCellType>> ListAsync(CancellationToken cancellationToken);

    Task<Dictionary<int, DbCellType>> ListByIdAsync(CancellationToken cancellationToken);

    Task<List<DbCellType>> ListByNamesAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken);

    Task<List<string>> ListNamesInUseAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken);

    Task<DbCellType?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    Task<bool> NameTakenAsync(string name, int? exceptId, CancellationToken cancellationToken);

    Task<int?> GetLastDisplayOrderAsync(CancellationToken cancellationToken);

    Task<int> GetDefaultIdAsync(CancellationToken cancellationToken);

    Task<Dictionary<int, int>> CountUsageByTypeAsync(CancellationToken cancellationToken);

    Task<int> CountUsageAsync(int cellTypeId, CancellationToken cancellationToken);

    Task<bool> HasSheetCellsAsync(int cellTypeId, CancellationToken cancellationToken);

    Task<bool> AnyOptionChosenAsync(IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken);

    void Add(DbCellType cellType);

    void Remove(DbCellType cellType);

    Task SaveAsync(CancellationToken cancellationToken);

    Task SaveClearingConfigurationOverridesAsync(int cellTypeId, CancellationToken cancellationToken);
}
