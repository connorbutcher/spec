using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>The revisions of sheet column blocks.</summary>
public interface ISheetColumnBlockRevisionRepository : ISheetRevisionRepository<SheetColumnBlockRevision>
{
    /// <summary>What the user sees of the table's column blocks: each one's current published revision and their own draft, read only.</summary>
    Task<List<SheetColumnBlockRevision>> ListVisibleOnTableAsync(int tableId, int userId, CancellationToken cancellationToken);

    /// <summary>The drafts of the table's column blocks that someone other than the user holds, or that wait for approval.</summary>
    Task<List<ContainedHold>> ListHoldsOnTableAsync(int tableId, int userId, CancellationToken cancellationToken);
}
