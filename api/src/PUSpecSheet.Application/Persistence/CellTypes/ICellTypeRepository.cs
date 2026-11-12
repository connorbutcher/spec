using PUSpecSheet.Database.Entities.CellTypes;

namespace PUSpecSheet.Application.Persistence.CellTypes;

/// <summary>The cell types template cells are made from, with their options, and where they are used.</summary>
public interface ICellTypeRepository
{
    /// <summary>Every cell type with its options, read only, in display order then name.</summary>
    Task<List<CellType>> ListWithOptionsAsync(CancellationToken cancellationToken);

    /// <summary>The cell type with its options, tracked for editing; null when there is no such type.</summary>
    Task<CellType?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    /// <summary>Whether another cell type than <paramref name="exceptId"/> already has the name.</summary>
    Task<bool> NameTakenAsync(string name, int? exceptId, CancellationToken cancellationToken);

    /// <summary>The highest display order in use, or null when there are no cell types.</summary>
    Task<int?> GetLastDisplayOrderAsync(CancellationToken cancellationToken);

    /// <summary>The type new cells get when none is chosen: the first Text type, else the first type; null when there are none.</summary>
    Task<int?> FindDefaultIdAsync(CancellationToken cancellationToken);

    /// <summary>How many template cells use each cell type that is used at all, by cell type id.</summary>
    Task<Dictionary<int, int>> CountUsageByTypeAsync(CancellationToken cancellationToken);

    /// <summary>How many template cells use the cell type.</summary>
    Task<int> CountUsageAsync(int cellTypeId, CancellationToken cancellationToken);

    /// <summary>Whether any sheet holds a cell made from the cell type.</summary>
    Task<bool> HasSheetCellsAsync(int cellTypeId, CancellationToken cancellationToken);

    /// <summary>Whether any of the options has been chosen on a sheet.</summary>
    Task<bool> AnyOptionChosenAsync(IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken);

    /// <summary>Clears the configuration overrides of every template cell of the type, straight in the database.</summary>
    Task ClearConfigurationOverridesAsync(int cellTypeId, CancellationToken cancellationToken);

    /// <summary>Every cell type with its options, read only, by id.</summary>
    Task<Dictionary<int, CellType>> GetAllWithOptionsByIdAsync(CancellationToken cancellationToken);

    /// <summary>Which of the names cell types already have.</summary>
    Task<List<string>> ListNamesInUseAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken);

    /// <summary>The cell types with any of the names, with their options, read only.</summary>
    Task<List<CellType>> ListByNamesAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken);

    void Add(CellType cellType);

    void Remove(CellType cellType);
}
