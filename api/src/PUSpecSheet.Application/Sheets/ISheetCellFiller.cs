namespace PUSpecSheet.Application.Sheets;

public interface ISheetCellFiller
{
    /// <summary>
    /// Makes sure every row of the table has a cell for each template cell of its template row, in every
    /// column block copy on the table. Safe to run any number of times and from several requests at once:
    /// it only adds what is missing, one request per table at a time.
    /// </summary>
    Task FillAsync(int tableId, CancellationToken cancellationToken);
}
