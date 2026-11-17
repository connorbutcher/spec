namespace PUSpecSheet.DataAccess.Sheets.Queries;

/// <summary>A sheet column block and the template column block it copies.</summary>
public sealed record SheetColumnBlockLink
{
    public required int Id { get; init; }

    public required int TemplateColumnBlockId { get; init; }
}
