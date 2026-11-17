using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Abstractions.CellTypes.Styles;

namespace PUSpecSheet.Abstractions.CellTypes;

/// <summary>
/// A cell type with the default configuration and style its cells start with and, for dropdowns, its
/// options. <see cref="Configuration"/> always matches <see cref="Kind"/>.
/// </summary>
public sealed record CellTypeDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required CellKind Kind { get; init; }

    public string? Description { get; init; }

    public required int DisplayOrder { get; init; }

    public required CellConfiguration Configuration { get; init; }

    public required CellStyle Style { get; init; }

    public required IReadOnlyList<CellTypeOptionDto> Options { get; init; }

    public required int UsageCount { get; init; }
}
