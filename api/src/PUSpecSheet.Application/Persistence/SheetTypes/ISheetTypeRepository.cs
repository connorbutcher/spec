using PUSpecSheet.Contracts.SheetTypes;
using PUSpecSheet.Domain.SheetTypes;

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

    void Add(SheetType sheetType);
}
