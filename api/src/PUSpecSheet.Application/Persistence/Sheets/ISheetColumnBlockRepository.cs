using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>The column blocks of horizontal sheet tables.</summary>
public interface ISheetColumnBlockRepository
{
    /// <summary>The column blocks of the sheet's tables with the template column block each copies, read only.</summary>
    Task<List<SheetColumnBlock>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>The column block with its template column block and table, read only; null when there is no such block.</summary>
    Task<SheetColumnBlock?> FindWithTemplateAndTableAsync(int id, CancellationToken cancellationToken);

    /// <summary>The table's column blocks and the template column block of each.</summary>
    Task<List<SheetColumnBlockLink>> ListLinksOnTableAsync(int tableId, CancellationToken cancellationToken);

    /// <summary>The ids of the table's column blocks.</summary>
    Task<List<int>> ListIdsOnTableAsync(int tableId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes column blocks nobody but their author ever saw, with their cells and any values typed into
    /// those cells, straight in the database.
    /// </summary>
    Task DeleteAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    void Add(SheetColumnBlock block);
}
