namespace PUSpecSheet.DataAccess.Templates;

public sealed record TemplateVersionKey
{
    public required int TableTemplateId { get; init; }

    public required int VersionNumber { get; init; }
}
