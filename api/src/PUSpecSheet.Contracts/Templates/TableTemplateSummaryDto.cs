using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>A table template without its sections, for lists. Orientation is the latest version's.</summary>
public sealed record TableTemplateSummaryDto(
    int Id,
    int SheetTypeId,
    string Name,
    int DisplayOrder,
    int LatestVersionNumber,
    TemplateOrientation Orientation);
