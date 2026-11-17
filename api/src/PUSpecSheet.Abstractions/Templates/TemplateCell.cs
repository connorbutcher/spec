using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.Abstractions.CellTypes.Styles;

namespace PUSpecSheet.Abstractions.Templates;

/// <summary>
/// A cell in a row. The overrides hold only what the cell changes from its cell type's defaults; the
/// cell uses the defaults with the overrides laid on top. A cell with a <see cref="ColumnBlockId"/>
/// belongs to that column block, and its column counts from the block's first column.
/// <see cref="LookupKey"/> is the name other applications look the cell up by, or null.
/// </summary>
public sealed record TemplateCell
{
    public required int Id { get; init; }

    public required int CellTypeId { get; init; }

    public required int Column { get; init; }

    public required int RowSpan { get; init; }

    public required int ColumnSpan { get; init; }

    public string? Caption { get; init; }

    public required bool IsRequired { get; init; }

    public CellConfiguration? ConfigurationOverride { get; init; }

    public CellStyle? StyleOverride { get; init; }

    public int? ColumnBlockId { get; init; }

    public string? LookupKey { get; init; }
}
