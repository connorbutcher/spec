using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// One sheet item's history while it is being built: its item row, the revisions written for it so far and the state
/// its latest published revision records, which the next copy is compared with.
/// </summary>
internal sealed class ItemTrack<TItem, TRevision>
    where TItem : class
    where TRevision : class, IDbSheetRevision
{
    public ItemTrack(TItem item, int? tableKey, int? groupKey)
    {
        Item = item;
        TableKey = tableKey;
        GroupKey = groupKey;
    }

    public TItem Item { get; }

    /// <summary>
    /// The table the item is in, for a group; null otherwise.
    /// </summary>
    public int? TableKey { get; }

    /// <summary>
    /// The group the item is in, for a row; null otherwise.
    /// </summary>
    public int? GroupKey { get; }

    public int RevisionCount { get; set; }

    public TRevision? LastPublished { get; set; }

    /// <summary>
    /// What <see cref="LastPublished"/> records, in the form <see cref="SheetHistoryBuilder"/> compares; null before
    /// the first publish.
    /// </summary>
    public string? PublishedState { get; set; }

    /// <summary>
    /// For a row, the cells and contents of <see cref="LastPublished"/>, which a revision removing the row keeps.
    /// </summary>
    public IReadOnlyList<(DbSheetTableCell Cell, CellContent Content)> PublishedCells { get; set; } = [];
}
