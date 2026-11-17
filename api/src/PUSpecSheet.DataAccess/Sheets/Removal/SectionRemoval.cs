namespace PUSpecSheet.DataAccess.Sheets.Removal;

/// <summary>A draft that removes a section: which section, who holds it, and whether it waits for approval.</summary>
public sealed record SectionRemoval
{
    public required int SheetSectionId { get; init; }

    public required int AuthorUserId { get; init; }

    public required bool AwaitingApproval { get; init; }
}
