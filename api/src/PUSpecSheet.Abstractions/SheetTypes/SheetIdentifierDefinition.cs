namespace PUSpecSheet.Abstractions.SheetTypes;

/// <summary>What the identity rules need to know about one identifier of a sheet type.</summary>
public sealed record SheetIdentifierDefinition
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required SheetIdentifierKind Kind { get; init; }

    public required bool IsRequired { get; init; }

    public required IReadOnlyList<string> Options { get; init; }
}
