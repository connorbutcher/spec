using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;
using PUSpecSheet.Domain.CellTypes.LabelledText;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>
/// The values row revisions hold. Values live in one typed table per cell kind, keyed by row revision and
/// sheet cell, because the row is what is versioned and locked; everything above this reads them as one
/// <see cref="CellValueBag"/> per cell.
/// </summary>
public interface IRowValueRepository
{
    /// <summary>The values of the row revisions, by revision id and then sheet cell id.</summary>
    Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadAsync(IReadOnlyCollection<int> revisionIds, CancellationToken cancellationToken);

    /// <summary>The values one row revision holds, by sheet cell id.</summary>
    Task<Dictionary<int, CellValueBag>> LoadAsync(int revisionId, CancellationToken cancellationToken);

    /// <summary>
    /// The values of every row revision on the sheet a view can show (see
    /// <see cref="ISheetRevisionRepository{TRevision}.ListInViewAsync"/>), by revision id and then sheet cell id.
    /// </summary>
    Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadInViewAsync(int sheetId, DateTime? moment, CancellationToken cancellationToken);

    /// <summary>The values of every row revision on the sheet published up to the moment (or ever, for none).</summary>
    Task<Dictionary<int, Dictionary<int, CellValueBag>>> LoadPublishedUpToAsync(int sheetId, DateTime? moment, CancellationToken cancellationToken);

    /// <summary>The settings chosen for cells in the row revisions, with the template cell and column block of each.</summary>
    Task<List<ChosenSettings>> ListChosenSettingsAsync(IReadOnlyCollection<int> revisionIds, CancellationToken cancellationToken);

    /// <summary>Copies every value and setting of one row revision to another, straight in the database.</summary>
    Task CopyAsync(int fromRevisionId, int toRevisionId, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the changed values in the revision: adds, updates, or removes the cell's value in its kind's
    /// table. The caller saves.
    /// </summary>
    Task SetManyAsync(int revisionId, IReadOnlyList<CellValueChange> changes, CancellationToken cancellationToken);

    /// <summary>
    /// Puts a labelled text cell's values in the order of its slots and drops values whose slot has gone.
    /// The caller saves.
    /// </summary>
    Task ArrangeSlotsAsync(int revisionId, int cellId, IReadOnlyList<LabelledTextSlot> slots, CancellationToken cancellationToken);

    /// <summary>Stores, replaces or clears the settings chosen for cells in the revision. The caller saves.</summary>
    Task SetSettingsAsync(int revisionId, IReadOnlyDictionary<int, CellInstanceSettings?> settingsByCell, CancellationToken cancellationToken);

    /// <summary>Adds settings for a cell in a revision. The caller saves.</summary>
    void AddSettings(int revisionId, int cellId, CellInstanceSettings settings);
}
