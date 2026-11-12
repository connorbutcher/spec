using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

public sealed class RowDrafts : DraftGateway<SheetRowRevision>
{
    public RowDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    protected override DbSet<SheetRowRevision> Revisions => Db.SheetRowRevisions;

    protected override string Subject => "This row";

    protected override int ItemIdOf(SheetRowRevision revision)
    {
        return revision.SheetRowId;
    }

    protected override IQueryable<SheetRowRevision> OfItems(IReadOnlyCollection<int> itemIds)
    {
        return Db.SheetRowRevisions.Where(revision => itemIds.Contains(revision.SheetRowId));
    }

    protected override IQueryable<SheetRowRevision> OfItem(int itemId)
    {
        return Db.SheetRowRevisions.Where(revision => revision.SheetRowId == itemId);
    }

    protected override void SetItem(SheetRowRevision revision, int itemId)
    {
        revision.SheetRowId = itemId;
    }

    /// <summary>A row is unchanged when its place and existence are, and every cell holds what is published.</summary>
    protected override async Task<HashSet<SheetRowRevision>> UnchangedAsync(
        IReadOnlyList<(SheetRowRevision Draft, SheetRowRevision Current)> pairs,
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
