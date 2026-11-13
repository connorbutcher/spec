using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Drafts;

public sealed class RowDrafts : DraftGateway<DbSheetRowRevision>
{
    public RowDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    protected override DbSet<DbSheetRowRevision> Revisions => Db.SheetRowRevisions;

    protected override string Subject => "This row";

    protected override int ItemIdOf(DbSheetRowRevision revision)
    {
        return revision.SheetRowId;
    }

    protected override IQueryable<DbSheetRowRevision> OfItems(IReadOnlyCollection<int> itemIds)
    {
        return Db.SheetRowRevisions.Where(revision => itemIds.Contains(revision.SheetRowId));
    }

    protected override IQueryable<DbSheetRowRevision> OfItem(int itemId)
    {
        return Db.SheetRowRevisions.Where(revision => revision.SheetRowId == itemId);
    }

    protected override void SetItem(DbSheetRowRevision revision, int itemId)
    {
        revision.SheetRowId = itemId;
    }

    /// <summary>A row is unchanged when its place and existence are, and every cell holds what is published.</summary>
    protected override async Task<HashSet<DbSheetRowRevision>> UnchangedAsync(
        IReadOnlyList<(DbSheetRowRevision Draft, DbSheetRowRevision Current)> pairs,
        CancellationToken cancellationToken)
    {
        var sameStructure = pairs
            .Where(pair => SheetRevisionComparer.SameStructure(pair.Draft, pair.Current))
            .ToList();
        if (sameStructure.Count == 0)
        {
            return [];
        }

        var revisionIds = sameStructure.SelectMany(pair => new[] { pair.Draft.Id, pair.Current.Id }).ToList();
        var held = await RowValueQueries.LoadAsync(Db, revisionIds, cancellationToken);
        return sameStructure
            .Where(pair => CellValuesComparer.Same(held.GetValueOrDefault(pair.Draft.Id), held.GetValueOrDefault(pair.Current.Id)))
            .Select(pair => pair.Draft)
            .ToHashSet();
    }
}
