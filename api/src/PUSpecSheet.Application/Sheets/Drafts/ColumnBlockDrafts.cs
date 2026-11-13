using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Drafts;

public sealed class ColumnBlockDrafts : DraftGateway<DbSheetColumnBlockRevision>
{
    public ColumnBlockDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    protected override DbSet<DbSheetColumnBlockRevision> Revisions => Db.SheetColumnBlockRevisions;

    protected override string Subject => "This column block";

    protected override int ItemIdOf(DbSheetColumnBlockRevision revision)
    {
        return revision.SheetColumnBlockId;
    }

    protected override IQueryable<DbSheetColumnBlockRevision> OfItems(IReadOnlyCollection<int> itemIds)
    {
        return Db.SheetColumnBlockRevisions.Where(revision => itemIds.Contains(revision.SheetColumnBlockId));
    }

    protected override IQueryable<DbSheetColumnBlockRevision> OfItem(int itemId)
    {
        return Db.SheetColumnBlockRevisions.Where(revision => revision.SheetColumnBlockId == itemId);
    }

    protected override void SetItem(DbSheetColumnBlockRevision revision, int itemId)
    {
        revision.SheetColumnBlockId = itemId;
    }
}
