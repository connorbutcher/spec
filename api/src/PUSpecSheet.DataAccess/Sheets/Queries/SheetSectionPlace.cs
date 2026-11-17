namespace PUSpecSheet.DataAccess.Sheets.Queries;

/// <summary>Where a sheet section sits: the template section it copies and the section it is inside, if any.</summary>
public sealed record SheetSectionPlace
{
    public SheetSectionPlace(int id, int templateSectionId, int? parentSheetSectionId)
    {
        Id = id;
        TemplateSectionId = templateSectionId;
        ParentSheetSectionId = parentSheetSectionId;
    }

    public int Id { get; init; }

    public int TemplateSectionId { get; init; }

    public int? ParentSheetSectionId { get; init; }
}
