namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;

public interface IRowTakeoverSettler
{
    Task GrantOverdueAsync(CancellationToken cancellationToken);

    Task CloseBlockedAsync(int sheetId, CancellationToken cancellationToken);
}
