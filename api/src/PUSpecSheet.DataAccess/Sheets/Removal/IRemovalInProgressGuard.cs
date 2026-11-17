namespace PUSpecSheet.DataAccess.Sheets.Removal;

public interface IRemovalInProgressGuard
{
    Task EnsureRowNotBeingRemovedAsync(int rowId, int userId, CancellationToken cancellationToken);

    Task EnsurePlaceNotBeingRemovedAsync(int tableId, int? sectionId, int userId, CancellationToken cancellationToken);
}
