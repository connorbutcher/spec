namespace PUSpecSheet.Database.Models.Sheets;

/// <summary>
/// A reference from an external system to a sheet, pushed in by that system rather than edited in the app.
/// What it says is versioned in <see cref="Revisions"/>, each tied to the sheet version it was pushed for.
/// </summary>
public class DbSheetExternalReference
{
    public int Id { get; set; }

    /// <summary>Stable identifier that stays with the reference across every version.</summary>
    public Guid PublicId { get; set; }

    public int SheetId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual DbSheet Sheet { get; set; } = null!;

    public virtual ICollection<DbSheetExternalReferenceRevision> Revisions { get; set; } = [];
}
