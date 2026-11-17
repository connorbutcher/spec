using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Abstractions.CellTypes.Styles;

namespace PUSpecSheet.Abstractions.CellTypes;

/// <summary>
/// A cell type with the default configuration and style its cells start with and, for dropdowns, its
/// options. <see cref="Configuration"/> always matches <see cref="Kind"/>.
/// </summary>
public sealed record CellTypeDto
{
    public CellTypeDto(
        int id,
        string name,
        CellKind kind,
        string? description,
        int displayOrder,
        CellConfiguration configuration,
        CellStyle style,
        IReadOnlyList<CellTypeOptionDto> options,
        int usageCount)
    {
        Id = id;
        Name = name;
        Kind = kind;
        Description = description;
        DisplayOrder = displayOrder;
        Configuration = configuration;
        Style = style;
        Options = options;
        UsageCount = usageCount;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public CellKind Kind { get; init; }

    public string? Description { get; init; }

    public int DisplayOrder { get; init; }

    public CellConfiguration Configuration { get; init; }

    public CellStyle Style { get; init; }

    public IReadOnlyList<CellTypeOptionDto> Options { get; init; }

    public int UsageCount { get; init; }
}
