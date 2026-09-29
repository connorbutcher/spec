using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Contracts.CellTypes;

/// <summary>
/// A cell type with the default configuration and style its cells start with and, for dropdowns, its
/// options. <see cref="Configuration"/> always matches <see cref="Kind"/>.
/// </summary>
public sealed record CellTypeDto(
    int Id,
    string Name,
    CellKind Kind,
    string? Description,
    int DisplayOrder,
    CellConfiguration Configuration,
    CellStyle Style,
    IReadOnlyList<CellTypeOptionDto> Options,
    int UsageCount);
