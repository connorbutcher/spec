using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Draft handling for sheet tables. A new draft also carries over the table's title.</summary>
public sealed class TableDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser) : DraftGateway<SheetTableRevision>(db, currentUser)
{
    protected override string Subject => "This table";

    protected override IQueryable<SheetTableRevision> CurrentAndDrafts(int itemId)
    {
        return Db.SheetTableRevisions.Where(revision => revision.SheetTableId == itemId && revision.SupersededAtUtc == null);
    }

    protected override Task<bool> IsUnchangedAsync(SheetTableRevision draft, SheetTableRevision current, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            SheetRevisionComparer.SameStructure(draft, current)
            && SheetRevisionComparer.SameText(draft.Title, current.Title));
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
