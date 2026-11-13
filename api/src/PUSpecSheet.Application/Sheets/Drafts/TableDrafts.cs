using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>Draft handling for sheet tables. A new draft also carries over the table's title.</summary>
public sealed class TableDrafts : DraftGateway<DbSheetTableRevision>
{
    public TableDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser)
        : base(db, currentUser)
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

    /// <summary>A table is unchanged when its place, existence and title are.</summary>
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
