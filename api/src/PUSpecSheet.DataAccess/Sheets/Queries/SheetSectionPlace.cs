namespace PUSpecSheet.DataAccess.Sheets.Queries;

public sealed record SheetSectionPlace
{
    public required int Id { get; init; }

    public required int TemplateSectionId { get; init; }

    public int? ParentSheetSectionId { get; init; }
}
