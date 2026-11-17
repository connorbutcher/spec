namespace PUSpecSheet.DataAccess.Sheets.Queries;

/// <summary>Where a sheet section sits: the template section it copies and the section it is inside, if any.</summary>
public sealed record SheetSectionPlace
{
    public required int Id { get; init; }

    public required int TemplateSectionId { get; init; }

    public int? ParentSheetSectionId { get; init; }
}
