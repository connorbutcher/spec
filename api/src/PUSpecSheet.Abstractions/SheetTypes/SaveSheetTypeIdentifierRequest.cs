using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.SheetTypes;

/// <summary>
/// Adds or changes an identifier of a sheet type. Its key is made from the name when it is added and
/// never changes. Its kind can't change once a sheet has a value for it.
/// </summary>
public sealed record SaveSheetTypeIdentifierRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    public required SheetIdentifierKind Kind { get; init; }

    public required bool IsRequired { get; init; }

    /// <summary>The choices of an option identifier, in order. Ignored for the other kinds.</summary>
    [MaxLength(100)]
    public IReadOnlyList<string>? Options { get; init; }
}
