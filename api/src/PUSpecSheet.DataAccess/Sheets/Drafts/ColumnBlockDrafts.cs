using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Drafts;

public sealed class ColumnBlockDrafts : DraftGateway<DbSheetColumnBlockRevision>
{
    public ColumnBlockDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser, IUserDataService userData)
        : base(db, currentUser, userData)
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
