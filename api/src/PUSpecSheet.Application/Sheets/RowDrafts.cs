using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Draft handling for sheet rows.</summary>
public sealed class RowDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser, RowValueStore valueStore) : DraftGateway<SheetRowRevision>(db, currentUser)
{
    protected override string Subject => "This row";

    protected override IQueryable<SheetRowRevision> CurrentAndDrafts(IReadOnlyCollection<int> itemIds)
    {
        return Db.SheetRowRevisions.Where(revision => itemIds.Contains(revision.SheetRowId) && revision.SupersededAtUtc == null);
    }

    protected override int ItemIdOf(SheetRowRevision revision)
    {
        return revision.SheetRowId;
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
        var values = await valueStore.LoadAsync(revisionIds, cancellationToken);
        return sameStructure
            .Where(pair => CellValuesComparer.Same(values.GetValueOrDefault(pair.Draft.Id), values.GetValueOrDefault(pair.Current.Id)))
            .Select(pair => pair.Draft)
            .ToHashSet();
    }

    protected override async Task<int> LastRevisionNumberAsync(int itemId, CancellationToken cancellationToken)
    {
        return await Db.SheetRowRevisions
            .Where(revision => revision.SheetRowId == itemId)
            .MaxAsync(revision => (int?)revision.RevisionNumber, cancellationToken) ?? 0;
    }

    protected override void Add(SheetRowRevision revision, int itemId)
    {
        revision.SheetRowId = itemId;
        Db.SheetRowRevisions.Add(revision);
    }
}
