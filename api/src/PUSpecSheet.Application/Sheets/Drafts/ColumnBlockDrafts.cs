using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

public sealed class ColumnBlockDrafts : DraftGateway<SheetColumnBlockRevision>
{
    public ColumnBlockDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    protected override DbSet<SheetColumnBlockRevision> Revisions => Db.SheetColumnBlockRevisions;

    protected override string Subject => "This column block";

    protected override int ItemIdOf(SheetColumnBlockRevision revision)
    {
        return revision.SheetColumnBlockId;
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
