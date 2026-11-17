using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Abstractions.CellTypes.LabelledText;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Values;

namespace PUSpecSheet.DataAccess.Sheets;

public interface ICellValueDataService
{
    Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadAsync(
        IReadOnlyCollection<int> revisionIds,
        CancellationToken cancellationToken);

    Task<Dictionary<int, CellValueBag>> LoadAsync(int revisionId, CancellationToken cancellationToken);

    Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadInViewAsync(int sheetId, DateTime? moment, CancellationToken cancellationToken);

    Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadPublishedUpToAsync(int sheetId, DateTime? moment, CancellationToken cancellationToken);

    Task<List<ChosenSettings>> ListChosenSettingsAsync(IReadOnlyCollection<int> revisionIds, CancellationToken cancellationToken);

    void AddSettings(int revisionId, int cellId, CellInstanceSettings settings);

    Task CopyAsync(int fromRevisionId, int toRevisionId, CancellationToken cancellationToken);

    Task SetManyAsync(int revisionId, IReadOnlyList<CellValueChange> changes, CancellationToken cancellationToken);

    Task ArrangeSlotsAsync(int revisionId, int cellId, IReadOnlyList<LabelledTextSlot> slots, CancellationToken cancellationToken);

    Task SetSettingsAsync(
        int revisionId,
        IReadOnlyDictionary<int, CellInstanceSettings?> settingsByCell,
        CancellationToken cancellationToken);

    Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadCellValuesAsync(
        RowRevisionScope revisions,
        CancellationToken cancellationToken);

    Task<Dictionary<int, CellValueBag>> LoadCellValuesAsync(
        int rowRevisionId,
        CancellationToken cancellationToken);
}
