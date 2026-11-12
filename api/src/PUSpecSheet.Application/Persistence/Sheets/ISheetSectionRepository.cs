using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>The sections of sheet tables.</summary>
public interface ISheetSectionRepository
{
    /// <summary>The sections of the sheet's tables with the template section each copies, read only.</summary>
    Task<List<SheetSection>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>The section with its template section and table, read only; null when there is no such section.</summary>
    Task<SheetSection?> FindWithTemplateAndTableAsync(int id, CancellationToken cancellationToken);

    /// <summary>The section, read only, when it is on the table; null otherwise.</summary>
    Task<SheetSection?> FindOnTableAsync(int id, int tableId, CancellationToken cancellationToken);

    /// <summary>Where each of the table's sections sits.</summary>
    Task<List<SheetSectionPlace>> ListPlacesOnTableAsync(int tableId, CancellationToken cancellationToken);

    /// <summary>The section each of the table's sections is inside (null at the top), by section id.</summary>
    Task<Dictionary<int, int?>> GetParentsOnTableAsync(int tableId, CancellationToken cancellationToken);

    /// <summary>The section each of the sections is inside (null at the top), by section id.</summary>
    Task<Dictionary<int, int?>> GetParentsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    /// <summary>The ids of the table's sections under one parent (or at the top level).</summary>
    Task<List<int>> ListSiblingIdsAsync(int tableId, int? parentSectionId, CancellationToken cancellationToken);

    /// <summary>Deletes the sections straight in the database; their rows cascade.</summary>
    Task DeleteAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    void Add(SheetSection section);
}
