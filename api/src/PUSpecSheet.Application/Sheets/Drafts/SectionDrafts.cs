using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

public sealed class SectionDrafts : DraftGateway<SheetSectionRevision>
{
    public SectionDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    protected override DbSet<SheetSectionRevision> Revisions => Db.SheetSectionRevisions;

    protected override string Subject => "This section";

    protected override int ItemIdOf(SheetSectionRevision revision)
    {
        return revision.SheetSectionId;
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
