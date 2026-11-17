using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.DataAccess.SheetTypes;

public interface ISheetTypeDataService
{
    Task<List<SheetType>> ListAsync(CancellationToken cancellationToken);

    Task<SheetType?> FindAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken);

    Task<int?> GetLastDisplayOrderAsync(CancellationToken cancellationToken);

    Task<List<int>> GetExistingIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    Task<int> GetIdByNameAsync(string name, CancellationToken cancellationToken);

    Task<List<int>> ListIdsByNamesAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken);

    Task<List<DbSheetTypeIdentifier>> ListIdentifiersForEditAsync(int sheetTypeId, CancellationToken cancellationToken);

    Task<List<SheetIdentifierDefinition>> ListIdentifierDefinitionsAsync(int sheetTypeId, CancellationToken cancellationToken);

    Task<bool> IsIdentifierInUseAsync(int identifierId, CancellationToken cancellationToken);

    void Add(DbSheetType sheetType);

    void AddIdentifier(DbSheetTypeIdentifier identifier);

    void RemoveIdentifier(DbSheetTypeIdentifier identifier);

    Task SaveAsync(CancellationToken cancellationToken);
}
