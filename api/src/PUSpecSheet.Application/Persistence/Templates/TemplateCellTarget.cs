namespace PUSpecSheet.Application.Persistence.Templates;

/// <summary>The template version a new cell's row is in, and the first free column after the row's cells.</summary>
public sealed record TemplateCellTarget(int TableTemplateVersionId, int NextColumn);
