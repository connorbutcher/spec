namespace PUSpecSheet.DataAccess.Templates;

public sealed record TemplateCellTarget
{
    public required int TableTemplateVersionId { get; init; }

    public required int NextColumn { get; init; }
}
