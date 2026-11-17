namespace PUSpecSheet.Abstractions.SheetTypes;

/// <summary>A sheet's value for one identifier, in its stored form (see <see cref="SheetIdentifierRules.TryNormalise"/>).</summary>
public sealed record SheetIdentifierValue
{
    public required SheetIdentifierDefinition Definition { get; init; }

    public required string Value { get; init; }
}
