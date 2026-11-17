using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Values;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets;

public interface ISheetCellDataService
{
    Task<List<NewCell>> ListNewCellsAsync(IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken);

    Task<List<RequiredCell>> ListRequiredAsync(IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken);

    Task<List<DbSheetCell>> ListOnCurrentRowsAsync(int sheetId, CancellationToken cancellationToken);

    Task<IReadOnlyList<(int SheetRowId, int TemplateCellId, int? SheetColumnBlockId)>> ListMissingAsync(int tableId, CancellationToken cancellationToken);
}
