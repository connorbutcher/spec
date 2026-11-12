using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Contracts.Templates;

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
        this.Id = id;
        this.SheetTypeId = sheetTypeId;
        this.Name = name;
        this.DisplayOrder = displayOrder;
        this.LatestVersionNumber = latestVersionNumber;
        this.Orientation = orientation;
    }

    public int Id { get; init; }

    public int SheetTypeId { get; init; }

    public string Name { get; init; }

    public int DisplayOrder { get; init; }

    public int LatestVersionNumber { get; init; }

    public TemplateOrientation Orientation { get; init; }
}
