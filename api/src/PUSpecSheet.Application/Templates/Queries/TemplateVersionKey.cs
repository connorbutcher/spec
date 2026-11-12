namespace PUSpecSheet.Application.Templates.Queries;

/// <summary>Which template a version belongs to, and its number there.</summary>
public sealed record TemplateVersionKey
{
    public TemplateVersionKey(int tableTemplateId, int versionNumber)
    {
        this.TableTemplateId = tableTemplateId;
        this.VersionNumber = versionNumber;
    }

    public int TableTemplateId { get; init; }

    public int VersionNumber { get; init; }
}
