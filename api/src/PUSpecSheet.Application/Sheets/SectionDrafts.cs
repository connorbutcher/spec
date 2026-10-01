using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Draft handling for sheet sections.</summary>
public sealed class SectionDrafts(PuSpecSheetDbContext db, ICurrentUser currentUser) : DraftGateway<SheetSectionRevision>(db, currentUser)
{
    protected override string Subject => "This section";

    protected override IQueryable<SheetSectionRevision> CurrentAndDrafts(int itemId)
    {
        return Db.SheetSectionRevisions.Where(revision => revision.SheetSectionId == itemId && revision.SupersededAtUtc == null);
    }

    protected override async Task<int> LastRevisionNumberAsync(int itemId, CancellationToken cancellationToken)
    {
        return await Db.SheetSectionRevisions
            .Where(revision => revision.SheetSectionId == itemId)
            .MaxAsync(revision => (int?)revision.RevisionNumber, cancellationToken) ?? 0;
    }

    protected override void Add(SheetSectionRevision revision, int itemId)
    {
        revision.SheetSectionId = itemId;
        Db.SheetSectionRevisions.Add(revision);
    }
}
