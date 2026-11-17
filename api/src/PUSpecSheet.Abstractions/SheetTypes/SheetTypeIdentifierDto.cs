namespace PUSpecSheet.Abstractions.SheetTypes;

/// <summary>
/// A value that tells one sheet of a sheet type from another within a phase, such as an engine
/// specification's serial number. <see cref="Key"/> names it in the published API's filters.
/// </summary>
public sealed record SheetTypeIdentifierDto
{
    public SheetTypeIdentifierDto(int id, string key, string name, SheetIdentifierKind kind, bool isRequired, int displayOrder, IReadOnlyList<string> options)
    {
        this.Id = id;
        this.Key = key;
        this.Name = name;
        this.Kind = kind;
        this.IsRequired = isRequired;
        this.DisplayOrder = displayOrder;
        this.Options = options;
    }

    public int Id { get; init; }

    public string Key { get; init; }

    public string Name { get; init; }

    public SheetIdentifierKind Kind { get; init; }

    public bool IsRequired { get; init; }

    public int DisplayOrder { get; init; }

    /// <summary>The choices of an option identifier; empty for the other kinds.</summary>
    public IReadOnlyList<string> Options { get; init; }
}
