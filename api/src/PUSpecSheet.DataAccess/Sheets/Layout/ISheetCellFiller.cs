namespace PUSpecSheet.DataAccess.Sheets.Layout;

public interface ISheetCellFiller
{
    Task FillAsync(int tableId, CancellationToken cancellationToken);
}
