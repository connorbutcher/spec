using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Contracts.CellTypes;

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
        this.Id = id;
        this.Name = name;
        this.Kind = kind;
        this.Description = description;
        this.DisplayOrder = displayOrder;
        this.Configuration = configuration;
        this.Style = style;
        this.Options = options;
        this.UsageCount = usageCount;
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
