using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Drafts;

public sealed class TableDrafts : DraftGateway<DbSheetTableRevision>
{
    public TableDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser, IUserDataService userData)
        : base(db, currentUser, userData)
    {
    }

    protected override DbSet<DbSheetTableRevision> Revisions => Db.SheetTableRevisions;

    protected override string Subject => "This table";

    protected override int ItemIdOf(DbSheetTableRevision revision)
    {
        return revision.SheetTableId;
    }

    protected override IQueryable<DbSheetTableRevision> OfItems(IReadOnlyCollection<int> itemIds)
    {
        return Db.SheetTableRevisions.Where(revision => itemIds.Contains(revision.SheetTableId));
    }

    protected override IQueryable<DbSheetTableRevision> OfItem(int itemId)
    {
        return Db.SheetTableRevisions.Where(revision => revision.SheetTableId == itemId);
    }

    protected override void SetItem(DbSheetTableRevision revision, int itemId)
    {
        revision.SheetTableId = itemId;
    }

    protected override Task<HashSet<DbSheetTableRevision>> UnchangedAsync(
        IReadOnlyList<(DbSheetTableRevision Draft, DbSheetTableRevision Current)> pairs,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(pairs
            .Where(pair => SheetRevisionComparer.SameStructure(pair.Draft, pair.Current)
                && SheetRevisionComparer.SameText(pair.Draft.Title, pair.Current.Title))
            .Select(pair => pair.Draft)
            .ToHashSet());
    }
}
