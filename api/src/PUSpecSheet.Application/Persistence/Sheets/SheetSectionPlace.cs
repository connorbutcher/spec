namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>Where a sheet section sits: the template section it copies and the section it is inside, if any.</summary>
public sealed record SheetSectionPlace
{
    public SheetSectionPlace(int id, int templateSectionId, int? parentSheetSectionId)
    {
        this.Id = id;
        this.TemplateSectionId = templateSectionId;
        this.ParentSheetSectionId = parentSheetSectionId;
    }

    public int Id { get; init; }

    public int TemplateSectionId { get; init; }

    public int? ParentSheetSectionId { get; init; }
}
