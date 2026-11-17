namespace PUSpecSheet.DataAccess.Templates.Queries;

/// <summary>The template version a new cell's row is in, and the first free column after the row's cells.</summary>
public sealed record TemplateCellTarget
{
    public required int TableTemplateVersionId { get; init; }

    public required int NextColumn { get; init; }
}
