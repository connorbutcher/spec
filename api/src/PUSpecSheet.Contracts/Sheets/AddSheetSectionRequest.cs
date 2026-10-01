namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// Adds a copy of an addable template section under a table (no parent) or under a section copy, which
/// must be a copy of the template section's own parent.
/// </summary>
public sealed record AddSheetSectionRequest(int TemplateSectionId, int? ParentSheetSectionId);
