namespace PUSpecSheet.DataAccess.Templates.Queries;

/// <summary>What decides whether a template version can still change: its number, its template's latest, and whether sheets use it.</summary>
public sealed record TemplateVersionState
{
    public required int VersionNumber { get; init; }

    public required int LatestNumber { get; init; }

    public required bool IsInUse { get; init; }
}
