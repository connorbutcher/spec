using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Removal;

public sealed class RemovalInProgressGuard(PuSpecSheetDbContext db) : IRemovalInProgressGuard
{
    public async Task EnsureRowNotBeingRemovedAsync(int rowId, int userId, CancellationToken cancellationToken)
    {
        var place = await db.SheetRows
            .Where(row => row.Id == rowId)
            .Select(row => new { row.SheetSectionId, row.SheetSection.SheetTableId })
            .SingleOrDefaultAsync(cancellationToken);
        if (place is null)
        {
            return;
        }

        await EnsurePlaceNotBeingRemovedAsync(place.SheetTableId, place.SheetSectionId, userId, cancellationToken);
    }

    public async Task EnsurePlaceNotBeingRemovedAsync(int tableId, int? sectionId, int userId, CancellationToken cancellationToken)
    {
        var removals = await TableRemovalsAsync(tableId, cancellationToken);
        if (sectionId is { } id)
        {
            removals.AddRange(await SectionRemovalsAroundAsync(tableId, id, cancellationToken));
        }

        if (PendingRemovals.Blocking(removals, userId) is not { } blocking)
        {
            return;
        }

        var name = await db.Users
            .Where(user => user.Id == blocking.AuthorUserId)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken) ?? "another user";
        throw new ConflictException(PendingRemovals.RefusalFor(blocking, name));
    }

    private async Task<List<PendingRemoval>> TableRemovalsAsync(int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetTableRevisions
            .Where(revision => revision.SheetTableId == tableId && revision.Status == RevisionStatus.Draft && revision.IsDeleted)
            .Select(revision => new PendingRemoval(RemovedContainer.Table, revision.AuthorUserId, revision.SheetSubmissionId != null))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Removals of the section or any section it sits in. Removals anywhere on the table are read first, as
    /// there are rarely any, and the section's ancestry only when there are.
    /// </summary>
    private async Task<List<PendingRemoval>> SectionRemovalsAroundAsync(int tableId, int sectionId, CancellationToken cancellationToken)
    {
        var onTable = await db.SheetSectionRevisions
            .Where(revision => revision.SheetSection.SheetTableId == tableId && revision.Status == RevisionStatus.Draft && revision.IsDeleted)
            .Select(revision => new { revision.SheetSectionId, revision.AuthorUserId, AwaitingApproval = revision.SheetSubmissionId != null })
            .ToListAsync(cancellationToken);
        if (onTable.Count == 0)
        {
            return [];
        }

        var parentOf = await db.SheetSections
            .Where(section => section.SheetTableId == tableId)
            .Select(section => new { section.Id, section.ParentSheetSectionId })
            .ToDictionaryAsync(section => section.Id, section => section.ParentSheetSectionId, cancellationToken);
        var around = new HashSet<int>();
        int? next = sectionId;
        while (next is { } current && around.Add(current))
        {
            next = parentOf.GetValueOrDefault(current);
        }

        return onTable
            .Where(removal => around.Contains(removal.SheetSectionId))
            .Select(removal => new PendingRemoval(RemovedContainer.Section, removal.AuthorUserId, removal.AwaitingApproval))
            .ToList();
    }
}
