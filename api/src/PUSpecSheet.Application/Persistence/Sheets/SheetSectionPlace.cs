namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>Where a sheet section sits: the template section it copies and the section it is inside, if any.</summary>
public sealed record SheetSectionPlace(int Id, int TemplateSectionId, int? ParentSheetSectionId);
