using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>Reads a sheet as the current user sees it. Every sheet change returns the refreshed live view.</summary>
public sealed class SheetReader(SheetSnapshotLoader loader, ICurrentUser currentUser)
{
    public async Task<SheetDto> ReadAsync(int sheetId, SheetViewPoint view, CancellationToken cancellationToken)
    {
        var snapshot = await loader.LoadAsync(sheetId, view, currentUser.UserId, cancellationToken);
        return new SheetViewBuilder(snapshot, currentUser.UserId).Build();
    }

    public Task<SheetDto> ReadLiveAsync(int sheetId, CancellationToken cancellationToken)
    {
        return ReadAsync(sheetId, SheetViewPoint.Live, cancellationToken);
    }
}
