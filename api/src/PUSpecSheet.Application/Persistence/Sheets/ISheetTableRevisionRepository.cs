using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>The revisions of sheet tables.</summary>
public interface ISheetTableRevisionRepository : ISheetRevisionRepository<SheetTableRevision>
{
    /// <summary>What the user sees of the sheet's tables: each one's current published revision and their own draft, read only.</summary>
    Task<List<SheetTableRevision>> ListVisibleOnSheetAsync(int sheetId, int userId, CancellationToken cancellationToken);

    /// <summary>The drafts that remove the table, whoever holds them.</summary>
    Task<List<PendingRemoval>> ListPendingRemovalsAsync(int tableId, CancellationToken cancellationToken);
}
