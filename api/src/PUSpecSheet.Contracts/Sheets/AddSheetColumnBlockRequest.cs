namespace PUSpecSheet.Contracts.Sheets;

/// <summary>Adds a copy of a template column block to a horizontal table, to the right of the existing blocks.</summary>
public sealed record AddSheetColumnBlockRequest(int TemplateColumnBlockId);
