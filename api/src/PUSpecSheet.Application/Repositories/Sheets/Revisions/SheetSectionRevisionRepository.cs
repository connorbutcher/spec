using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Database.Entities.Sheets;
using PUSpecSheet.Domain.Sheets;

using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Repositories.Sheets.Revisions;

public sealed class SheetSectionRevisionRepository
    : SheetRevisionRepository<SheetSectionRevision>, ISheetSectionRevisionRepository
{
    public SheetSectionRevisionRepository(PuSpecSheetDbContext db)
        : base(db)
    {
    }

    protected override DbSet<SheetSectionRevision> Revisions => Db.SheetSectionRevisions;

    protected override Expression<Func<SheetSectionRevision, int>> ItemId => revision => revision.SheetSectionId;

    public async Task<List<SheetSectionRevision>> ListVisibleOnTableAsync(int tableId, int userId, CancellationToken cancellationToken)
    {
        return await Db.SheetSectionRevisions
            .Where(revision => revision.SheetSection.SheetTableId == tableId)
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SectionRemoval>> ListPendingRemovalsOnTableAsync(int tableId, CancellationToken cancellationToken)
    {
        return await Db.SheetSectionRevisions
            .Where(revision => revision.SheetSection.SheetTableId == tableId && revision.Status == RevisionStatus.Draft && revision.IsDeleted)
            .Select(revision => new SectionRemoval(revision.SheetSectionId, revision.AuthorUserId, revision.SheetSubmissionId != null))
            .ToListAsync(cancellationToken);
    }

    public Task<List<ContainedHold>> ListHoldsAsync(IReadOnlyCollection<int> sectionIds, int userId, CancellationToken cancellationToken)
    {
        return ListHoldsAsync(Db.SheetSectionRevisions.Where(revision => sectionIds.Contains(revision.SheetSectionId)), userId, cancellationToken);
    }

    public Task<List<ContainedHold>> ListHoldsOnTableAsync(int tableId, int userId, CancellationToken cancellationToken)
    {
        return ListHoldsAsync(Db.SheetSectionRevisions.Where(revision => revision.SheetSection.SheetTableId == tableId), userId, cancellationToken);
    }

    protected override IQueryable<SheetSectionRevision> OfItems(IReadOnlyCollection<int> itemIds)
    {
        return Db.SheetSectionRevisions.Where(revision => itemIds.Contains(revision.SheetSectionId));
    }

    protected override IQueryable<SheetSectionRevision> OfItem(int itemId)
    {
        return Db.SheetSectionRevisions.Where(revision => revision.SheetSectionId == itemId);
    }

    protected override void SetItem(SheetSectionRevision revision, int itemId)
    {
        revision.SheetSectionId = itemId;
    }
}
