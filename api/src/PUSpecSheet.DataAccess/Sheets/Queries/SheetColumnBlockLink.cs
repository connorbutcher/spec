namespace PUSpecSheet.DataAccess.Sheets.Queries;

/// <summary>A sheet column block and the template column block it copies.</summary>
public sealed record SheetColumnBlockLink
{
    public SheetColumnBlockLink(int id, int templateColumnBlockId)
    {
        Id = id;
        TemplateColumnBlockId = templateColumnBlockId;
    }

    public int Id { get; init; }

    public int TemplateColumnBlockId { get; init; }
}
