using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Draft handling for sheet rows.</summary>
public sealed class RowDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser, RowValueStore valueStore) : DraftGateway<SheetRowRevision>(db, currentUser)
{
    protected override string Subject => "This row";

    protected override IQueryable<SheetRowRevision> CurrentAndDrafts(int itemId)
    {
        return Db.SheetRowRevisions.Where(revision => revision.SheetRowId == itemId && revision.SupersededAtUtc == null);
    }

    protected override async Task<bool> IsUnchangedAsync(SheetRowRevision draft, SheetRowRevision current, CancellationToken cancellationToken)
    {
        if (!SheetRevisionComparer.SameStructure(draft, current))
        {
            return false;
        }

        var values = await valueStore.LoadAsync([draft.Id, current.Id], cancellationToken);
        return CellValuesComparer.Same(values.GetValueOrDefault(draft.Id), values.GetValueOrDefault(current.Id));
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
