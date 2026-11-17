namespace PUSpecSheet.Abstractions.SheetTypes;

/// <summary>A sheet's value for one identifier, in its stored form (see <see cref="SheetIdentifierRules.TryNormalise"/>).</summary>
public sealed record SheetIdentifierValue
{
    public SheetIdentifierValue(SheetIdentifierDefinition definition, string value)
    {
        Definition = definition;
        Value = value;
    }

    public SheetIdentifierDefinition Definition { get; init; }

    public string Value { get; init; }
}
