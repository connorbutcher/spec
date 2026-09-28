using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>A table template with its whole section tree, rows and cells, each level in display order.</summary>
public sealed record TableTemplateDto(
    int Id,
    int SheetTypeId,
    string Name,
    int DisplayOrder,
    TemplateOrientation Orientation,
    IReadOnlyList<TemplateSectionDto> Sections);
