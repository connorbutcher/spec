using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>A new sheet's value for one of its sheet type's identifiers. Blank leaves an optional identifier empty.</summary>
public sealed record SheetIdentifierValueRequest
{
    public SheetIdentifierValueRequest(int identifierId, [MaxLength(200)] string? value)
    {
        IdentifierId = identifierId;
        Value = value;
    }

    public int IdentifierId { get; init; }

    public string? Value { get; init; }
}
