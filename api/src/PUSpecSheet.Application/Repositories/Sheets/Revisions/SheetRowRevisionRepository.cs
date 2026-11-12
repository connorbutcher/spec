using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Database.Entities.Sheets;
using PUSpecSheet.Domain.Sheets;

using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Repositories.Sheets.Revisions;

public sealed class SheetRowRevisionRepository
    : SheetRevisionRepository<SheetRowRevision>, ISheetRowRevisionRepository
{
    public SheetRowRevisionRepository(PuSpecSheetDbContext db)
        : base(db)
    {
    }

    protected override DbSet<SheetRowRevision> Revisions => Db.SheetRowRevisions;

    protected override Expression<Func<SheetRowRevision, int>> ItemId => revision => revision.SheetRowId;

    public async Task<List<SheetRowRevision>> ListVisibleInSectionAsync(int sectionId, int userId, CancellationToken cancellationToken)
    {
        return await Db.SheetRowRevisions
            .Where(revision => revision.SheetRow.SheetSectionId == sectionId)
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SheetRowRevision>> ListVisibleAsync(IReadOnlyCollection<int> rowIds, int userId, CancellationToken cancellationToken)
    {
        return await Db.SheetRowRevisions
            .Where(revision => rowIds.Contains(revision.SheetRowId))
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SheetRowRevision>> ListCurrentAndDraftAsync(int rowId, CancellationToken cancellationToken)
    {
        return await Db.SheetRowRevisions
            .Where(revision => revision.SheetRowId == rowId)
            .CurrentAndDrafts()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<SheetRowRevision?> FindDraftForEditAsync(int rowId, CancellationToken cancellationToken)
    {
        return await Db.SheetRowRevisions
            .SingleOrDefaultAsync(
                revision => revision.SheetRowId == rowId && revision.Status == RevisionStatus.Draft,
                cancellationToken);
    }

    public async Task<int?> FindDraftAuthorAsync(int rowId, CancellationToken cancellationToken)
    {
        return await Db.SheetRowRevisions
            .Where(revision => revision.SheetRowId == rowId && revision.Status == RevisionStatus.Draft)
            .Select(revision => (int?)revision.AuthorUserId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<List<ContainedHold>> ListHoldsInSectionsAsync(IReadOnlyCollection<int> sectionIds, int userId, CancellationToken cancellationToken)
    {
        return ListHoldsAsync(Db.SheetRowRevisions.Where(revision => sectionIds.Contains(revision.SheetRow.SheetSectionId)), userId, cancellationToken);
    }

    public Task<List<ContainedHold>> ListHoldsOnTableAsync(int tableId, int userId, CancellationToken cancellationToken)
    {
        return ListHoldsAsync(Db.SheetRowRevisions.Where(revision => revision.SheetRow.SheetSection.SheetTableId == tableId), userId, cancellationToken);
    }

    protected override IQueryable<SheetRowRevision> OfItems(IReadOnlyCollection<int> itemIds)
    {
        return Db.SheetRowRevisions.Where(revision => itemIds.Contains(revision.SheetRowId));
    }

    protected override IQueryable<SheetRowRevision> OfItem(int itemId)
    {
        return Db.SheetRowRevisions.Where(revision => revision.SheetRowId == itemId);
    }

    protected override void SetItem(SheetRowRevision revision, int itemId)
    {
        revision.SheetRowId = itemId;
    }
}
