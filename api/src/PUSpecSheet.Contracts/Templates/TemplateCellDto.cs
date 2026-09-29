using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// A cell in a row. The overrides hold only what the cell changes from its cell type's defaults; the
/// cell uses the defaults with the overrides laid on top.
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
    CellStyle? StyleOverride);
