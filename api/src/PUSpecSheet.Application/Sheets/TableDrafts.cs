using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Draft handling for sheet tables. A new draft also carries over the table's title.</summary>
public sealed class TableDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser) : DraftGateway<SheetTableRevision>(db, currentUser)
{
    protected override string Subject => "This table";

    protected override IQueryable<SheetTableRevision> CurrentAndDrafts(IReadOnlyCollection<int> itemIds)
    {
        return Db.SheetTableRevisions.Where(revision => itemIds.Contains(revision.SheetTableId) && revision.SupersededAtUtc == null);
    }

    protected override int ItemIdOf(SheetTableRevision revision)
    {
        return revision.SheetTableId;
    }

    /// <summary>A table is unchanged when its place, existence and title are.</summary>
    protected override Task<HashSet<SheetTableRevision>> UnchangedAsync(
        IReadOnlyList<(SheetTableRevision Draft, SheetTableRevision Current)> pairs,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(pairs
            .Where(pair => SheetRevisionComparer.SameStructure(pair.Draft, pair.Current)
                && SheetRevisionComparer.SameText(pair.Draft.Title, pair.Current.Title))
            .Select(pair => pair.Draft)
            .ToHashSet());
    }

    protected override async Task<int> LastRevisionNumberAsync(int itemId, CancellationToken cancellationToken)
    {
        return await Db.SheetTableRevisions
            .Where(revision => revision.SheetTableId == itemId)
            .MaxAsync(revision => (int?)revision.RevisionNumber, cancellationToken) ?? 0;
    }

    protected override void Add(SheetTableRevision revision, int itemId)
    {
        revision.SheetTableId = itemId;
        Db.SheetTableRevisions.Add(revision);
    }
}
