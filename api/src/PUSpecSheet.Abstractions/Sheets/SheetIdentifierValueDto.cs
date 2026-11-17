namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>A sheet's value for one of its sheet type's identifiers.</summary>
public sealed record SheetIdentifierValueDto
{
    public required int IdentifierId { get; init; }

    public required string Value { get; init; }
}
