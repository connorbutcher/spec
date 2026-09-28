using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>A table template without its sections, for lists.</summary>
public sealed record TableTemplateSummaryDto(
    int Id,
    int SheetTypeId,
    string Name,
    int DisplayOrder,
    TemplateOrientation Orientation);
