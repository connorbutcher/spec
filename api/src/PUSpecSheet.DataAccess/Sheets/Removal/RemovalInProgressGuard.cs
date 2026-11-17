using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Sheets.Removal;

public sealed class RemovalInProgressGuard : IRemovalInProgressGuard
{
    private readonly PuSpecSheetDbContext _db;
    private readonly IUserDataService _userData;

    public RemovalInProgressGuard(PuSpecSheetDbContext db, IUserDataService userData)
    {
        _db = db;
        _userData = userData;
    }

    public async Task EnsureRowNotBeingRemovedAsync(int rowId, int userId, CancellationToken cancellationToken)
    {
        var place = await SheetRowQueries.FindPlaceAsync(_db, rowId, cancellationToken);
        if (place is null)
        {
            return;
        }

        await EnsurePlaceNotBeingRemovedAsync(place.SheetTableId, place.SheetSectionId, userId, cancellationToken);
    }

    public async Task EnsurePlaceNotBeingRemovedAsync(int tableId, int? sectionId, int userId, CancellationToken cancellationToken)
    {
        var removals = await SheetTableRevisionQueries.ListPendingRemovalsAsync(_db, tableId, cancellationToken);
        if (sectionId is { } id)
        {
            removals.AddRange(await SectionRemovalsAroundAsync(tableId, id, cancellationToken));
        }

        if (PendingRemovals.Blocking(removals, userId) is not { } blocking)
        {
            return;
        }

        var name = await _userData.FindDisplayNameAsync(blocking.AuthorUserId, cancellationToken) ?? "another user";
        throw new ConflictException(PendingRemovals.RefusalFor(blocking, name));
    }


    /// <summary>
    /// Removals of the section or any section it sits in. Removals anywhere on the table are read first, as
    /// there are rarely any, and the section's ancestry only when there are.
    /// </summary>
    private async Task<List<PendingRemoval>> SectionRemovalsAroundAsync(int tableId, int sectionId, CancellationToken cancellationToken)
    {
        var onTable = await SheetSectionRevisionQueries.ListPendingRemovalsOnTableAsync(_db, tableId, cancellationToken);
        if (onTable.Count == 0)
        {
            return [];
        }

        var parentOf = await SheetSectionQueries.GetParentsOnTableAsync(_db, tableId, cancellationToken);
        var around = new HashSet<int>();
        int? next = sectionId;
        while (next is { } current && around.Add(current))
        {
            next = parentOf.GetValueOrDefault(current);
        }

        return onTable
            .Where(removal => around.Contains(removal.SheetSectionId))
            .Select(removal => new PendingRemoval
            {
                Container = RemovedContainer.Section,
                AuthorUserId = removal.AuthorUserId,
                AwaitingApproval = removal.AwaitingApproval,
            })
            .ToList();
    }
}
