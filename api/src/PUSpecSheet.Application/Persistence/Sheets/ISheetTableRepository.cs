using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>The tables on sheets.</summary>
public interface ISheetTableRepository
{
    /// <summary>The sheet's tables with the template version and template each is built from, read only.</summary>
    Task<List<SheetTable>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>The table, tracked for editing; null when there is no such table.</summary>
    Task<SheetTable?> FindForEditAsync(int id, CancellationToken cancellationToken);

    /// <summary>The table with its template version, read only; null when there is no such table.</summary>
    Task<SheetTable?> FindWithTemplateVersionAsync(int id, CancellationToken cancellationToken);

    /// <summary>The sheet the table is on; null when there is no such table.</summary>
    Task<int?> FindSheetIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>The ids of every table on the sheet.</summary>
    Task<List<int>> ListIdsOnSheetAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>Deletes the table straight in the database; everything on it cascades.</summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken);

    void Add(SheetTable table);
}
