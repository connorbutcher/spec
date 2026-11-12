using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>The rows of sheet sections.</summary>
public interface ISheetRowRepository
{
    /// <summary>
    /// The sheet's rows with their cells, read only. The cells' template cells are not loaded: the caller
    /// attaches them from template rows it reads anyway, rather than reading each again for every cell.
    /// </summary>
    Task<List<SheetRow>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>
    /// The row with its section and table, and its cells with their template cells and cell types (and the
    /// types' options when asked), read only; null when there is no such row.
    /// </summary>
    Task<SheetRow?> FindWithCellTypesAsync(int id, bool withOptions, CancellationToken cancellationToken);

    /// <summary>The row with its section, the section's template section and table, read only; null when there is no such row.</summary>
    Task<SheetRow?> FindWithSectionAsync(int id, CancellationToken cancellationToken);

    /// <summary>The sheet the row is on; null when there is no such row.</summary>
    Task<int?> FindSheetIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>The section the row is in; null when there is no such row.</summary>
    Task<int?> FindSectionIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>The row's section and table; null when there is no such row.</summary>
    Task<SheetRowPlace?> FindPlaceAsync(int id, CancellationToken cancellationToken);

    /// <summary>The ids of the section's rows.</summary>
    Task<List<int>> ListIdsInSectionAsync(int sectionId, CancellationToken cancellationToken);

    /// <summary>The ids of the rows in the sections.</summary>
    Task<List<int>> ListIdsInSectionsAsync(IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken);

    /// <summary>The ids of the table's rows.</summary>
    Task<List<int>> ListIdsOnTableAsync(int tableId, CancellationToken cancellationToken);

    /// <summary>The template rows the rows are made from.</summary>
    Task<List<int>> ListTemplateRowIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    /// <summary>The table's rows made from any of the template rows, with the template row of each, by row id.</summary>
    Task<Dictionary<int, int>> GetTemplateRowsOnTableAsync(int tableId, IReadOnlyCollection<int> templateRowIds, CancellationToken cancellationToken);

    /// <summary>Deletes the row straight in the database; its cells, revisions and values cascade.</summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken);

    void Add(SheetRow row);
}
