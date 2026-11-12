using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>Sheets, one per phase and sheet type, and the versions published of them.</summary>
public interface ISheetRepository
{
    /// <summary>The sheet, read only; null when there is no such sheet.</summary>
    Task<Sheet?> FindAsync(int id, CancellationToken cancellationToken);

    /// <summary>The sheet, tracked for editing; null when there is no such sheet.</summary>
    Task<Sheet?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    /// <summary>The id of the phase's sheet of the sheet type; null when it hasn't been created yet.</summary>
    Task<int?> FindIdAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken);

    /// <summary>The sheet's versions with who published each, read only, oldest first.</summary>
    Task<List<SheetVersion>> ListVersionsAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>The highest version number of the sheet, or 0 when nothing has been published.</summary>
    Task<int> GetLastVersionNumberAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the sheet's tables, sections, rows and column blocks that have no revisions left (they only
    /// ever existed as drafts that are gone), straight in the database.
    /// </summary>
    Task DeleteItemsWithoutRevisionsAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>Whether any sheet has a version published with the note.</summary>
    Task<bool> AnyVersionWithNoteAsync(string note, CancellationToken cancellationToken);

    void Add(Sheet sheet);

    void AddVersion(SheetVersion version);
}
