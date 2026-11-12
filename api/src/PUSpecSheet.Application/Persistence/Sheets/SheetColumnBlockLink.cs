namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>A sheet column block and the template column block it copies.</summary>
public sealed record SheetColumnBlockLink(int Id, int TemplateColumnBlockId);
