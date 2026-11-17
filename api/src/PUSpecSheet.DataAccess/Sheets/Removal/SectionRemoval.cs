namespace PUSpecSheet.DataAccess.Sheets.Removal;

public sealed record SectionRemoval
{
    public required int SheetSectionId { get; init; }

    public required int AuthorUserId { get; init; }

    public required bool AwaitingApproval { get; init; }
}
