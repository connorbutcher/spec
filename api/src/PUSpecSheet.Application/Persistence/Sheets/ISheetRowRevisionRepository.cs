using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>The revisions of sheet rows.</summary>
public interface ISheetRowRevisionRepository : ISheetRevisionRepository<SheetRowRevision>
{
    /// <summary>What the user sees of the section's rows: each one's current published revision and their own draft, read only.</summary>
    Task<List<SheetRowRevision>> ListVisibleInSectionAsync(int sectionId, int userId, CancellationToken cancellationToken);

    /// <summary>What the user sees of the rows: each one's current published revision and their own draft, read only.</summary>
    Task<List<SheetRowRevision>> ListVisibleAsync(IReadOnlyCollection<int> rowIds, int userId, CancellationToken cancellationToken);

    /// <summary>The row's current published revision and its draft, whoever holds it, read only.</summary>
    Task<List<SheetRowRevision>> ListCurrentAndDraftAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>The row's draft, tracked; null when it has none.</summary>
    Task<SheetRowRevision?> FindDraftForEditAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>Who holds the row's draft; null when it has none.</summary>
    Task<int?> FindDraftAuthorAsync(int rowId, CancellationToken cancellationToken);

    /// <summary>The drafts of rows in the sections that someone other than the user holds, or that wait for approval.</summary>
    Task<List<ContainedHold>> ListHoldsInSectionsAsync(IReadOnlyCollection<int> sectionIds, int userId, CancellationToken cancellationToken);

    /// <summary>The drafts of the table's rows that someone other than the user holds, or that wait for approval.</summary>
    Task<List<ContainedHold>> ListHoldsOnTableAsync(int tableId, int userId, CancellationToken cancellationToken);
}
