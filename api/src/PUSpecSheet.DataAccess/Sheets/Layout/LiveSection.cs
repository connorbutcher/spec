namespace PUSpecSheet.DataAccess.Sheets.Layout;

public sealed record LiveSection
{
    public required int Id { get; init; }

    public required int TemplateSectionId { get; init; }

    public int? ParentSheetSectionId { get; init; }

    public required int DisplayOrder { get; init; }
}
