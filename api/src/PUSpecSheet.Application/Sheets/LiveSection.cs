namespace PUSpecSheet.Application.Sheets;

/// <summary>A section copy that exists in the current user's live view, for counting copies and ordering them.</summary>
public sealed record LiveSection(int Id, int TemplateSectionId, int? ParentSheetSectionId, int DisplayOrder);
