namespace PUSpecSheet.Application.Templates.Queries;

/// <summary>What decides whether a template version can still change: its number, its template's latest, and whether sheets use it.</summary>
public sealed record TemplateVersionState
{
    public TemplateVersionState(int versionNumber, int latestNumber, bool isInUse)
    {
        this.VersionNumber = versionNumber;
        this.LatestNumber = latestNumber;
        this.IsInUse = isInUse;
    }

    public int VersionNumber { get; init; }

    public int LatestNumber { get; init; }

    public bool IsInUse { get; init; }
}
