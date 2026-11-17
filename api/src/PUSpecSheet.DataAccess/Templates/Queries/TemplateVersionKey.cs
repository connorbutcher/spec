namespace PUSpecSheet.DataAccess.Templates.Queries;

/// <summary>Which template a version belongs to, and its number there.</summary>
public sealed record TemplateVersionKey
{
    public required int TableTemplateId { get; init; }

    public required int VersionNumber { get; init; }
}
