using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// A cell in a row. The overrides hold only what the cell changes from its cell type's defaults; the
/// cell uses the defaults with the overrides laid on top. A cell with a <see cref="ColumnBlockId"/>
/// belongs to that column block, and its column counts from the block's first column.
/// <see cref="LookupKey"/> is the name other applications look the cell up by, or null.
/// </summary>
public sealed record TemplateCellDto(
    int Id,
    int CellTypeId,
    int Column,
    int RowSpan,
    int ColumnSpan,
    string? Caption,
    bool IsRequired,
    CellConfiguration? ConfigurationOverride,
    CellStyle? StyleOverride,
    int? ColumnBlockId = null,
    string? LookupKey = null);
