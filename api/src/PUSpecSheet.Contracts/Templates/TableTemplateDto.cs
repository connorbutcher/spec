using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// A table template at one version, with that version's whole section tree, rows and cells, each level
/// in display order, and for a horizontal table its column blocks left to right. <see cref="IsEditable"/>
/// is true only for the latest version while no sheet uses it.
/// </summary>
public sealed record TableTemplateDto(
    int Id,
    int SheetTypeId,
    string Name,
    int DisplayOrder,
    int VersionId,
    int VersionNumber,
    TemplateOrientation Orientation,
    bool IsEditable,
    IReadOnlyList<TableTemplateVersionSummaryDto> Versions,
    IReadOnlyList<TemplateSectionDto> Sections,
    IReadOnlyList<TemplateColumnBlockDto> ColumnBlocks);
