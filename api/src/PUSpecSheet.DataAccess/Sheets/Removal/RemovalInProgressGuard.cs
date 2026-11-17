using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Users;

namespace PUSpecSheet.DataAccess.Sheets.Removal;

public sealed class RemovalInProgressGuard : IRemovalInProgressGuard
{
    private readonly IUserDataService _userData;
    private readonly ISheetTableDataService _sheetTableData;
    private readonly ISheetSectionDataService _sheetSectionData;
    private readonly ISheetRowDataService _sheetRowData;

    public RemovalInProgressGuard(
        IUserDataService userData,
        ISheetTableDataService sheetTableData,
        ISheetSectionDataService sheetSectionData,
        ISheetRowDataService sheetRowData)
    {
        _userData = userData;
        _sheetTableData = sheetTableData;
        _sheetSectionData = sheetSectionData;
        _sheetRowData = sheetRowData;
    }

    public async Task EnsureRowNotBeingRemovedAsync(int rowId, int userId, CancellationToken cancellationToken)
    {
        var place = await _sheetRowData.FindPlaceAsync(rowId, cancellationToken);
        if (place is null)
        {
            return;
        }

        await EnsurePlaceNotBeingRemovedAsync(place.SheetTableId, place.SheetSectionId, userId, cancellationToken);
    }

    public async Task EnsurePlaceNotBeingRemovedAsync(int tableId, int? sectionId, int userId, CancellationToken cancellationToken)
    {
        var removals = await _sheetTableData.ListPendingRemovalsAsync(tableId, cancellationToken);
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


    private async Task<List<PendingRemoval>> SectionRemovalsAroundAsync(int tableId, int sectionId, CancellationToken cancellationToken)
    {
        var onTable = await _sheetSectionData.ListPendingRemovalsOnTableAsync(tableId, cancellationToken);
        if (onTable.Count == 0)
        {
            return [];
        }

        var parentOf = await _sheetSectionData.GetParentsOnTableAsync(tableId, cancellationToken);
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
