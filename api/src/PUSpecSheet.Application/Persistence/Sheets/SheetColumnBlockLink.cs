namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>A sheet column block and the template column block it copies.</summary>
public sealed record SheetColumnBlockLink
{
    public SheetColumnBlockLink(int id, int templateColumnBlockId)
    {
        this.Id = id;
        this.TemplateColumnBlockId = templateColumnBlockId;
    }

    public int Id { get; init; }

    public int TemplateColumnBlockId { get; init; }
}
