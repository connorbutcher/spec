namespace PUSpecSheet.DataAccess.Sheets.Layout;

/// <summary>A section copy that exists in the current user's live view, for counting copies and ordering them.</summary>
public sealed record LiveSection
{
    public required int Id { get; init; }

    public required int TemplateSectionId { get; init; }

    public int? ParentSheetSectionId { get; init; }

    public required int DisplayOrder { get; init; }
}
