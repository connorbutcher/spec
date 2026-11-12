using PUSpecSheet.Domain.Phases;

namespace PUSpecSheet.Application.Persistence.Phases;

/// <summary>Phases, such as V6 and its builds, and the sheet types each one has.</summary>
public interface IPhaseRepository
{
    /// <summary>Every phase with its sheet types, read only, in display order then code.</summary>
    Task<List<Phase>> ListWithSheetTypesAsync(CancellationToken cancellationToken);

    /// <summary>Every phase with its sheet types, tracked for editing.</summary>
    Task<List<Phase>> ListForEditAsync(CancellationToken cancellationToken);

    /// <summary>The phase with its sheet types, read only; null when there is no such phase.</summary>
    Task<Phase?> FindWithSheetTypesAsync(int id, CancellationToken cancellationToken);

    /// <summary>The phase with its sheet types, tracked for editing; null when there is no such phase.</summary>
    Task<Phase?> FindForEditAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);

    /// <summary>The highest display order among the children of a parent (or the top level), or null when there are none.</summary>
    Task<int?> GetLastDisplayOrderAsync(int? parentPhaseId, CancellationToken cancellationToken);

    /// <summary>
    /// The name of the first sheet type, in display order, among those asked for whose sheet in the phase
    /// already has tables; null when none has.
    /// </summary>
    Task<string?> FindSheetTypeWithTablesAsync(int phaseId, IReadOnlyCollection<int> sheetTypeIds, CancellationToken cancellationToken);

    /// <summary>Whether the phase offers the sheet type.</summary>
    Task<bool> HasSheetTypeAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken);

    void Add(Phase phase);
}
