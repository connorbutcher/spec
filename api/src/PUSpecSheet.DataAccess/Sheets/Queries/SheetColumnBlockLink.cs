namespace PUSpecSheet.DataAccess.Sheets.Queries;

public sealed record SheetColumnBlockLink
{
    public required int Id { get; init; }

    public required int TemplateColumnBlockId { get; init; }
}
