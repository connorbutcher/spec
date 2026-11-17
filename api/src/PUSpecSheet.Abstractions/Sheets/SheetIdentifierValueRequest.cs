using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>A new sheet's value for one of its sheet type's identifiers. Blank leaves an optional identifier empty.</summary>
public sealed record SheetIdentifierValueRequest
{
    public required int IdentifierId { get; init; }

    [MaxLength(200)]
    public string? Value { get; init; }
}
