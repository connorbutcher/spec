using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets;

public interface ISheetDataService
{
    Task<DbSheet?> FindAsync(int id, CancellationToken cancellationToken);

    Task<DbSheet?> FindWithIdentifiersAsync(int id, CancellationToken cancellationToken);

    Task<DbSheet?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    Task<int?> FindIdAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken);

    Task<List<DbSheet>> ListWithIdentifiersAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken);

    Task<List<DbSheetVersion>> ListVersionsAsync(int sheetId, CancellationToken cancellationToken);

    Task<int> GetLastVersionNumberAsync(int sheetId, CancellationToken cancellationToken);

    Task<bool> AnyVersionWithNoteAsync(string note, CancellationToken cancellationToken);

    Task<List<SheetSummary>> ListAsync(int phaseId, int? sheetId, CancellationToken cancellationToken);
}
