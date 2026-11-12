namespace PUSpecSheet.Application.Persistence.Templates;

/// <summary>What decides whether a template version can still change: its number, its template's latest, and whether sheets use it.</summary>
public sealed record TemplateVersionState(int VersionNumber, int LatestNumber, bool IsInUse);
