using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Values;

namespace PUSpecSheet.DataAccess.Sheets;

public interface ISheetCellDataService
{
    Task<List<NewCell>> ListNewCellsAsync(IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken);

    Task<List<RequiredCell>> ListRequiredAsync(IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken);
}
