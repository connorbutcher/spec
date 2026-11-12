namespace PUSpecSheet.Application.Sheets.Removal;

/// <summary>A draft that removes a section: which section, who holds it, and whether it waits for approval.</summary>
public sealed record SectionRemoval
{
    public SectionRemoval(int sheetSectionId, int authorUserId, bool awaitingApproval)
    {
        this.SheetSectionId = sheetSectionId;
        this.AuthorUserId = authorUserId;
        this.AwaitingApproval = awaitingApproval;
    }

    public int SheetSectionId { get; init; }

    public int AuthorUserId { get; init; }

    public bool AwaitingApproval { get; init; }
}
