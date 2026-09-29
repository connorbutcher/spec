using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Contracts.CellTypes;

/// <summary>
/// Creates or replaces a cell type. A <see cref="Configuration"/> for a different kind than
/// <see cref="Kind"/>, or none, starts the kind with nothing set. <see cref="Options"/> is the full list
/// of dropdown choices, in order, and is ignored for other kinds.
/// </summary>
public sealed record SaveCellTypeRequest(
    [Required, MaxLength(100)] string Name,
    CellKind Kind,
    [MaxLength(500)] string? Description,
    CellConfiguration? Configuration,
    CellStyle? Style,
    IReadOnlyList<string>? Options);
