using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>
/// Creates a sheet of a sheet type in a phase, told apart from the phase's other sheets of the type by its
/// identifier values, which are fixed from then on. With <see cref="CopyFromSheetId"/> it starts with the
/// published content of that sheet, of the same type, as the creator's drafts.
/// </summary>
public sealed record CreateSheetRequest
{
    public required int SheetTypeId { get; init; }

    [Required, MaxLength(20)]
    public required IReadOnlyList<SheetIdentifierValueRequest> Identifiers { get; init; }

    public int? CopyFromSheetId { get; init; }
}
