using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Draft handling for sheet column blocks.</summary>
public sealed class ColumnBlockDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser) : DraftGateway<SheetColumnBlockRevision>(db, currentUser)
{
    protected override string Subject => "This column block";

    protected override IQueryable<SheetColumnBlockRevision> CurrentAndDrafts(IReadOnlyCollection<int> itemIds)
    {
        return Db.SheetColumnBlockRevisions.Where(revision => itemIds.Contains(revision.SheetColumnBlockId)).CurrentAndDrafts();
    }

    protected override int ItemIdOf(SheetColumnBlockRevision revision)
    {
        return revision.SheetColumnBlockId;
    }

    protected override async Task<int> LastRevisionNumberAsync(int itemId, CancellationToken cancellationToken)
    {
        return await Db.SheetColumnBlockRevisions
            .Where(revision => revision.SheetColumnBlockId == itemId)
            .MaxAsync(revision => (int?)revision.RevisionNumber, cancellationToken) ?? 0;
    }

    protected override void Add(SheetColumnBlockRevision revision, int itemId)
    {
        revision.SheetColumnBlockId = itemId;
        Db.SheetColumnBlockRevisions.Add(revision);
    }
}
