using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// A cell in a row. The overrides hold only what the cell changes from its cell type's defaults; the
/// cell uses the defaults with the overrides laid on top. A cell with a <see cref="ColumnBlockId"/>
/// belongs to that column block, and its column counts from the block's first column.
/// <see cref="LookupKey"/> is the name other applications look the cell up by, or null.
/// </summary>
public sealed record TemplateCellDto
{
    public TemplateCellDto(
        int id,
        int cellTypeId,
        int column,
        int rowSpan,
        int columnSpan,
        string? caption,
        bool isRequired,
        CellConfiguration? configurationOverride,
        CellStyle? styleOverride,
        int? columnBlockId = null,
        string? lookupKey = null)
    {
        this.Id = id;
        this.CellTypeId = cellTypeId;
        this.Column = column;
        this.RowSpan = rowSpan;
        this.ColumnSpan = columnSpan;
        this.Caption = caption;
        this.IsRequired = isRequired;
        this.ConfigurationOverride = configurationOverride;
        this.StyleOverride = styleOverride;
        this.ColumnBlockId = columnBlockId;
        this.LookupKey = lookupKey;
    }

    public int Id { get; init; }

    public int CellTypeId { get; init; }

    public int Column { get; init; }

    public int RowSpan { get; init; }

    public int ColumnSpan { get; init; }

    public string? Caption { get; init; }

    public bool IsRequired { get; init; }

    public CellConfiguration? ConfigurationOverride { get; init; }

    public CellStyle? StyleOverride { get; init; }

    public int? ColumnBlockId { get; init; }

    public string? LookupKey { get; init; }
}
