namespace PUSpecSheet.Abstractions.SheetTypes;

/// <summary>What the identity rules need to know about one identifier of a sheet type.</summary>
public sealed record SheetIdentifierDefinition
{
    public SheetIdentifierDefinition(int id, string name, SheetIdentifierKind kind, bool isRequired, IReadOnlyList<string> options)
    {
        this.Id = id;
        this.Name = name;
        this.Kind = kind;
        this.IsRequired = isRequired;
        this.Options = options;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public SheetIdentifierKind Kind { get; init; }

    public bool IsRequired { get; init; }

    public IReadOnlyList<string> Options { get; init; }
}
