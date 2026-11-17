namespace PUSpecSheet.Abstractions.Templates;

/// <summary>A table template without its sections, for lists. Orientation is the latest version's.</summary>
public sealed record TableTemplateSummaryDto
{
    public required int Id { get; init; }

    public required int SheetTypeId { get; init; }

    public required string Name { get; init; }

    public required int DisplayOrder { get; init; }

    public required int LatestVersionNumber { get; init; }

    public required TemplateOrientation Orientation { get; init; }
}
