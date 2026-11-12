namespace PUSpecSheet.Application.Sheets.Removal;

/// <summary>A draft that removes a section: which section, who holds it, and whether it waits for approval.</summary>
public sealed record SectionRemoval(int SheetSectionId, int AuthorUserId, bool AwaitingApproval);
