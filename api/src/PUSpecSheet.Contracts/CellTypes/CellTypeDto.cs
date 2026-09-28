using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Contracts.CellTypes;

/// <summary>A cell type with its kind-specific settings and, for dropdowns, its options.</summary>
public sealed record CellTypeDto(
    int Id,
    string Name,
    CellKind Kind,
    string? Description,
    int DisplayOrder,
    int? MaxLength,
    int? DecimalPlaces,
    decimal? MinValue,
    decimal? MaxValue,
    string? Unit,
    IReadOnlyList<CellTypeOptionDto> Options,
    int UsageCount);
