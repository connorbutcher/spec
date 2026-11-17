using PUSpecSheet.Abstractions.SheetTypes;

namespace PUSpecSheet.Database.Models.SheetTypes;

/// <summary>
/// A value that tells one sheet of a sheet type from another within a phase, such as an engine
/// specification's serial number. Each sheet gives its value when it is created and keeps it.
/// </summary>
public class DbSheetTypeIdentifier
{
    public int Id { get; set; }

    public int SheetTypeId { get; set; }

    /// <summary>Name used by the published API's filters, such as <c>serialNumber</c>. Fixed once created.</summary>
    public required string Key { get; set; }

    public required string Name { get; set; }

    public SheetIdentifierKind Kind { get; set; }

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>The choices of an <see cref="SheetIdentifierKind.Option"/> identifier; null for the other kinds.</summary>
    public List<string>? Options { get; set; }

    public virtual DbSheetType SheetType { get; set; } = null!;
}
