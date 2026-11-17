using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Abstractions.CellTypes.Styles;

namespace PUSpecSheet.Abstractions.CellTypes;

/// <summary>
/// Creates or replaces a cell type. A <see cref="Configuration"/> for a different kind than
/// <see cref="Kind"/>, or none, starts the kind with nothing set. <see cref="Options"/> is the full list
/// of dropdown choices, in order, and is ignored for other kinds.
/// </summary>
public sealed record SaveCellTypeRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    public required CellKind Kind { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    public CellConfiguration? Configuration { get; init; }

    public CellStyle? Style { get; init; }

    public IReadOnlyList<string>? Options { get; init; }
}
