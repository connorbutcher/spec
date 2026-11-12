namespace PUSpecSheet.Application.Sheets.Layout;

/// <summary>A section copy that exists in the current user's live view, for counting copies and ordering them.</summary>
public sealed record LiveSection
{
    public LiveSection(int id, int templateSectionId, int? parentSheetSectionId, int displayOrder)
    {
        this.Id = id;
        this.TemplateSectionId = templateSectionId;
        this.ParentSheetSectionId = parentSheetSectionId;
        this.DisplayOrder = displayOrder;
    }

    public int Id { get; init; }

    public int TemplateSectionId { get; init; }

    public int? ParentSheetSectionId { get; init; }

    public int DisplayOrder { get; init; }
}
