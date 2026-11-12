using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Users.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Removal;

public sealed class RemovalInProgressGuard : IRemovalInProgressGuard
{
    private readonly PuSpecSheetDbContext db;

    public RemovalInProgressGuard(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task EnsureRowNotBeingRemovedAsync(int rowId, int userId, CancellationToken cancellationToken)
    {
        var place = await SheetRowQueries.FindPlaceAsync(db, rowId, cancellationToken);
        if (place is null)
        {
            return;
        }

        await EnsurePlaceNotBeingRemovedAsync(place.SheetTableId, place.SheetSectionId, userId, cancellationToken);
    }

    public async Task EnsurePlaceNotBeingRemovedAsync(int tableId, int? sectionId, int userId, CancellationToken cancellationToken)
    {
        var removals = await SheetTableRevisionQueries.ListPendingRemovalsAsync(db, tableId, cancellationToken);
        if (sectionId is { } id)
        {
            removals.AddRange(await SectionRemovalsAroundAsync(tableId, id, cancellationToken));
        }

        if (PendingRemovals.Blocking(removals, userId) is not { } blocking)
        {
            return;
        }

        var name = await UserQueries.FindDisplayNameAsync(db, blocking.AuthorUserId, cancellationToken) ?? "another user";
        throw new ConflictException(PendingRemovals.RefusalFor(blocking, name));
    }


    /// <summary>
    /// Removals of the section or any section it sits in. Removals anywhere on the table are read first, as
    /// there are rarely any, and the section's ancestry only when there are.
    /// </summary>
    private async Task<List<PendingRemoval>> SectionRemovalsAroundAsync(int tableId, int sectionId, CancellationToken cancellationToken)
    {
        var onTable = await SheetSectionRevisionQueries.ListPendingRemovalsOnTableAsync(db, tableId, cancellationToken);
        if (onTable.Count == 0)
        {
            return [];
        }

        var parentOf = await SheetSectionQueries.GetParentsOnTableAsync(db, tableId, cancellationToken);
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
