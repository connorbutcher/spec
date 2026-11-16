using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.Database.Models.Sheets;

/// <summary>A sheet's value for one of its sheet type's identifiers, set when the sheet is created and never changed.</summary>
public class DbSheetIdentifierValue
{
    public int SheetId { get; set; }

    public int SheetTypeIdentifierId { get; set; }

    /// <summary>The value in its stored form: trimmed text, an invariant number, a yyyy-MM-dd date or the option's own spelling.</summary>
    public string Value { get; set; } = string.Empty;

    public virtual DbSheet Sheet { get; set; } = null!;

    public virtual DbSheetTypeIdentifier Identifier { get; set; } = null!;
}
