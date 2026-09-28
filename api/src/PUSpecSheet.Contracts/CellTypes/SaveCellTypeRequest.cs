using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Contracts.CellTypes;

/// <summary>
/// Creates or replaces a cell type. Settings that don't apply to <see cref="Kind"/> are cleared.
/// <see cref="Options"/> is the full list of dropdown choices, in order.
/// </summary>
public sealed record SaveCellTypeRequest(
    [Required, MaxLength(100)] string Name,
    CellKind Kind,
    [MaxLength(500)] string? Description,
    [Range(1, 4000)] int? MaxLength,
    [Range(0, 6)] int? DecimalPlaces,
    decimal? MinValue,
    decimal? MaxValue,
    [MaxLength(20)] string? Unit,
    IReadOnlyList<string>? Options);
