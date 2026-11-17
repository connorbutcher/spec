using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Users;

namespace PUSpecSheet.DataAccess.Sheets.Reading;

public sealed class SheetReader
{
    private readonly SheetSnapshotLoader _loader;
    private readonly ICurrentUser _currentUser;

    public SheetReader(SheetSnapshotLoader loader, ICurrentUser currentUser)
    {
        _loader = loader;
        _currentUser = currentUser;
    }

    public async Task<Sheet> ReadAsync(int sheetId, SheetViewPoint view, CancellationToken cancellationToken)
    {
        var snapshot = await _loader.LoadAsync(sheetId, view, _currentUser.UserId, cancellationToken);
        return new SheetViewBuilder(snapshot, _currentUser.UserId).Build();
    }

    public Task<Sheet> ReadLiveAsync(int sheetId, CancellationToken cancellationToken)
    {
        return ReadAsync(sheetId, SheetViewPoint.Live, cancellationToken);
    }
}
