using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Repositories.Sheets.Revisions;

public sealed class SheetColumnBlockRevisionRepository
    : SheetRevisionRepository<SheetColumnBlockRevision>, ISheetColumnBlockRevisionRepository
{
    public SheetColumnBlockRevisionRepository(PuSpecSheetDbContext db)
        : base(db)
    {
    }

    protected override DbSet<SheetColumnBlockRevision> Revisions => Db.SheetColumnBlockRevisions;

    protected override Expression<Func<SheetColumnBlockRevision, int>> ItemId => revision => revision.SheetColumnBlockId;

    public async Task<List<SheetColumnBlockRevision>> ListVisibleOnTableAsync(int tableId, int userId, CancellationToken cancellationToken)
    {
        return await Db.SheetColumnBlockRevisions
            .Where(revision => revision.SheetColumnBlock.SheetTableId == tableId)
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<ContainedHold>> ListHoldsOnTableAsync(int tableId, int userId, CancellationToken cancellationToken)
    {
        return ListHoldsAsync(Db.SheetColumnBlockRevisions.Where(revision => revision.SheetColumnBlock.SheetTableId == tableId), userId, cancellationToken);
    }

    protected override IQueryable<SheetColumnBlockRevision> OfItems(IReadOnlyCollection<int> itemIds)
    {
        return Db.SheetColumnBlockRevisions.Where(revision => itemIds.Contains(revision.SheetColumnBlockId));
    }

    protected override IQueryable<SheetColumnBlockRevision> OfItem(int itemId)
    {
        return Db.SheetColumnBlockRevisions.Where(revision => revision.SheetColumnBlockId == itemId);
    }

    protected override void SetItem(SheetColumnBlockRevision revision, int itemId)
    {
        revision.SheetColumnBlockId = itemId;
    }
}
