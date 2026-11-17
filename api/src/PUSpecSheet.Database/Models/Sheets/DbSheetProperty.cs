using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.Database.Models.Sheets;

/// <summary>
/// A property of a sheet: a value about the sheet as a whole rather than a cell in one of its tables. Its value,
/// position and whether it's on the sheet are versioned in <see cref="Revisions"/> the same way as tables and
/// rows: edited as a draft, published with the sheet's next version, at most one revision per version.
/// </summary>
public class DbSheetProperty
{
    public int Id { get; set; }

    /// <summary>Stable identifier that stays with the property across every version.</summary>
    public Guid PublicId { get; set; }

    public int SheetId { get; set; }

    public int SheetTypePropertyId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual DbSheet Sheet { get; set; } = null!;

    public virtual DbSheetTypeProperty SheetTypeProperty { get; set; } = null!;

    public virtual ICollection<DbSheetPropertyRevision> Revisions { get; set; } = [];
}
