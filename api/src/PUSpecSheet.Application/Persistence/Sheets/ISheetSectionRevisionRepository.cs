using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>The revisions of sheet sections.</summary>
public interface ISheetSectionRevisionRepository : ISheetRevisionRepository<SheetSectionRevision>
{
    /// <summary>What the user sees of the table's sections: each one's current published revision and their own draft, read only.</summary>
    Task<List<SheetSectionRevision>> ListVisibleOnTableAsync(int tableId, int userId, CancellationToken cancellationToken);

    /// <summary>The drafts that remove a section of the table, whoever holds them.</summary>
    Task<List<SectionRemoval>> ListPendingRemovalsOnTableAsync(int tableId, CancellationToken cancellationToken);

    /// <summary>The drafts of the sections that someone other than the user holds, or that wait for approval.</summary>
    Task<List<ContainedHold>> ListHoldsAsync(IReadOnlyCollection<int> sectionIds, int userId, CancellationToken cancellationToken);

    /// <summary>The drafts of the table's sections that someone other than the user holds, or that wait for approval.</summary>
    Task<List<ContainedHold>> ListHoldsOnTableAsync(int tableId, int userId, CancellationToken cancellationToken);
}
