namespace PUSpecSheet.Contracts.Sheets;

/// <summary>A kind of row that can be added to a section, named after its first caption.</summary>
public sealed record AddableRowDto(int TemplateRowId, string Label);
