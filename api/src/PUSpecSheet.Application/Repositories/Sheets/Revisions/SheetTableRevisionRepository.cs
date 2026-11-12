using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Database.Entities.Sheets;
using PUSpecSheet.Domain.Sheets;

using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Repositories.Sheets.Revisions;

public sealed class SheetTableRevisionRepository
    : SheetRevisionRepository<SheetTableRevision>, ISheetTableRevisionRepository
{
    public SheetTableRevisionRepository(PuSpecSheetDbContext db)
        : base(db)
    {
    }

    protected override DbSet<SheetTableRevision> Revisions => Db.SheetTableRevisions;

    protected override Expression<Func<SheetTableRevision, int>> ItemId => revision => revision.SheetTableId;

    public async Task<List<SheetTableRevision>> ListVisibleOnSheetAsync(int sheetId, int userId, CancellationToken cancellationToken)
    {
        return await Db.SheetTableRevisions
            .Where(revision => revision.SheetTable.SheetId == sheetId)
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PendingRemoval>> ListPendingRemovalsAsync(int tableId, CancellationToken cancellationToken)
    {
        return await Db.SheetTableRevisions
            .Where(revision => revision.SheetTableId == tableId && revision.Status == RevisionStatus.Draft && revision.IsDeleted)
            .Select(revision => new PendingRemoval(RemovedContainer.Table, revision.AuthorUserId, revision.SheetSubmissionId != null))
            .ToListAsync(cancellationToken);
    }

    protected override IQueryable<SheetTableRevision> OfItems(IReadOnlyCollection<int> itemIds)
    {
        return Db.SheetTableRevisions.Where(revision => itemIds.Contains(revision.SheetTableId));
    }

    protected override IQueryable<SheetTableRevision> OfItem(int itemId)
    {
        return Db.SheetTableRevisions.Where(revision => revision.SheetTableId == itemId);
    }

    protected override void SetItem(SheetTableRevision revision, int itemId)
    {
        revision.SheetTableId = itemId;
    }
}
