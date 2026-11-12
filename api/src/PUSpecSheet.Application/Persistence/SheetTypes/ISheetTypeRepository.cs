using PUSpecSheet.Contracts.SheetTypes;
using PUSpecSheet.Database.Entities.SheetTypes;

namespace PUSpecSheet.Application.Persistence.SheetTypes;

/// <summary>The kinds of sheet a phase can have, such as Specification or Parts.</summary>
public interface ISheetTypeRepository
{
    /// <summary>Every sheet type, in display order then name.</summary>
    Task<List<SheetTypeDto>> ListAsync(CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken);

    /// <summary>The highest display order in use, or null when there are no sheet types.</summary>
    Task<int?> GetLastDisplayOrderAsync(CancellationToken cancellationToken);

    /// <summary>Which of the ids asked for are sheet types.</summary>
    Task<List<int>> GetExistingIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    /// <summary>The id of the sheet type with the name. It must exist.</summary>
    Task<int> GetIdByNameAsync(string name, CancellationToken cancellationToken);

    /// <summary>The ids of the sheet types with any of the names.</summary>
    Task<List<int>> ListIdsByNamesAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken);

    void Add(SheetType sheetType);
}
