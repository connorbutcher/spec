using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Drafts;

public sealed class SectionDrafts : DraftGateway<DbSheetSectionRevision>
{
    public SectionDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    protected override DbSet<DbSheetSectionRevision> Revisions => Db.SheetSectionRevisions;

    protected override string Subject => "This section";

    protected override int ItemIdOf(DbSheetSectionRevision revision)
    {
        return revision.SheetSectionId;
    }

    protected override IQueryable<DbSheetSectionRevision> OfItems(IReadOnlyCollection<int> itemIds)
    {
        return Db.SheetSectionRevisions.Where(revision => itemIds.Contains(revision.SheetSectionId));
    }

    protected override IQueryable<DbSheetSectionRevision> OfItem(int itemId)
    {
        return Db.SheetSectionRevisions.Where(revision => revision.SheetSectionId == itemId);
    }

    protected override void SetItem(DbSheetSectionRevision revision, int itemId)
    {
        revision.SheetSectionId = itemId;
    }
}
