using PUSpecSheet.Database.Models.Phases;

namespace PUSpecSheet.DataAccess.Phases;

public interface IPhaseDataService
{
    Task<List<DbPhase>> ListAsync(CancellationToken cancellationToken);

    Task<List<DbPhase>> ListForEditAsync(CancellationToken cancellationToken);

    Task<List<DbPhase>> ListByCodesForEditAsync(IReadOnlyCollection<string> codes, CancellationToken cancellationToken);

    Task<DbPhase?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);

    Task<int?> GetLastDisplayOrderAsync(int? parentPhaseId, CancellationToken cancellationToken);

    Task<bool> HasSheetTypeAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken);

    Task<string?> FindSheetTypeWithTablesAsync(int phaseId, IReadOnlyCollection<int> sheetTypeIds, CancellationToken cancellationToken);

    void Add(DbPhase phase);

    Task SaveAsync(CancellationToken cancellationToken);
}
