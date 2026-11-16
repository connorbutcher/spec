using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>A new sheet's value for one of its sheet type's identifiers. Blank leaves an optional identifier empty.</summary>
public sealed record SheetIdentifierValueRequest
{
    public SheetIdentifierValueRequest(int identifierId, [MaxLength(200)] string? value)
    {
        this.IdentifierId = identifierId;
        this.Value = value;
    }

    public int IdentifierId { get; init; }

    public string? Value { get; init; }
}
