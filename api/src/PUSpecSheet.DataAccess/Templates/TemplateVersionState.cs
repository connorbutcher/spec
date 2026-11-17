namespace PUSpecSheet.DataAccess.Templates;

public sealed record TemplateVersionState
{
    public required int VersionNumber { get; init; }

    public required int LatestNumber { get; init; }

    public required bool IsInUse { get; init; }
}
