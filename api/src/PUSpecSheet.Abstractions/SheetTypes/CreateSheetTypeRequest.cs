using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.SheetTypes;

/// <summary>Adds a sheet type to the end of the list.</summary>
public sealed record CreateSheetTypeRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }
}
