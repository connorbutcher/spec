using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Migrator.Snapshots;

namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// One sheet item's history while it is being built: its identity row, the revisions written for it so far and
/// the state its latest published revision records, which the next copy is compared with.
/// </summary>
internal sealed class ItemTrack<TIdentity, TRevision>
    where TIdentity : class
    where TRevision : class, IDbSheetRevision
{
    public ItemTrack(TIdentity identity, int? tableKey, int? sectionKey)
    {
        Identity = identity;
        TableKey = tableKey;
        SectionKey = sectionKey;
    }

    public TIdentity Identity { get; }

    /// <summary>The table the item is in, for a column block or section; null otherwise.</summary>
    public int? TableKey { get; }

    /// <summary>The section the item is in, for a row or nested section; null otherwise.</summary>
    public int? SectionKey { get; }

    /// <summary>The public id of the item in the latest copy it is in, which the identity row keeps.</summary>
    public Guid PublicId { get; set; }

    public int RevisionCount { get; set; }

    public TRevision? LastPublished { get; set; }

    /// <summary>What <see cref="LastPublished"/> records, in the form <see cref="SheetHistoryBuilder"/> compares; null before the first publish.</summary>
    public string? PublishedState { get; set; }

    /// <summary>For a row, the cells and contents of <see cref="LastPublished"/>, which a revision removing the row keeps.</summary>
    public IReadOnlyList<(DbSheetCell Cell, CellContent Content)> PublishedCells { get; set; } = [];
}
