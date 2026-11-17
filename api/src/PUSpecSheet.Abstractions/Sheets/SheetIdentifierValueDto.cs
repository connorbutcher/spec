namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>A sheet's value for one of its sheet type's identifiers.</summary>
public sealed record SheetIdentifierValueDto
{
    public SheetIdentifierValueDto(int identifierId, string value)
    {
        IdentifierId = identifierId;
        Value = value;
    }

    public int IdentifierId { get; init; }

    public string Value { get; init; }
}
