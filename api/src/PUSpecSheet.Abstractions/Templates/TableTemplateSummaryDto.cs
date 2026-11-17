namespace PUSpecSheet.Abstractions.Templates;

/// <summary>A table template without its sections, for lists. Orientation is the latest version's.</summary>
public sealed record TableTemplateSummaryDto
{
    public TableTemplateSummaryDto(
        int id,
        int sheetTypeId,
        string name,
        int displayOrder,
        int latestVersionNumber,
        TemplateOrientation orientation)
    {
        Id = id;
        SheetTypeId = sheetTypeId;
        Name = name;
        DisplayOrder = displayOrder;
        LatestVersionNumber = latestVersionNumber;
        Orientation = orientation;
    }

    public int Id { get; init; }

    public int SheetTypeId { get; init; }

    public string Name { get; init; }

    public int DisplayOrder { get; init; }

    public int LatestVersionNumber { get; init; }

    public TemplateOrientation Orientation { get; init; }
}
